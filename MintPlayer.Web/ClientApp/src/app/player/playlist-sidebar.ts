import { ChangeDetectionStrategy, Component, PLATFORM_ID, computed, effect, inject, input, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BsListGroupComponent, BsListGroupItemComponent } from '@mintplayer/ng-bootstrap/list-group';
import { BsProgressComponent, BsProgressBarComponent } from '@mintplayer/ng-bootstrap/progress-bar';
import { BsFormComponent, BsFormControlDirective } from '@mintplayer/ng-bootstrap/form';
import { BsBadgeComponent } from '@mintplayer/ng-bootstrap/badge';
import { Color } from '@mintplayer/ng-bootstrap';
import { ERepeatMode } from '@mintplayer/playlist-controller';
import { MediaPlayabilityService } from '../media/media-playability.service';
import { playlistEntryFromUrl } from './media-resolver';
import { PlayerService } from './player.service';

/**
 * The docked play-queue panel. A right-side drawer (open/closed driven by {@link PlayerService.isOpen})
 * that lets the user steer the one shared queue: transport (shuffle, repeat, previous/next, play/pause),
 * a progress bar, the queue list with a now-playing highlight, per-row remove + catalog deep-link, and an
 * "Add URL" box gated on {@link MediaPlayabilityService.canPlay}. Mounted once in the shell beside the
 * floating card; both read/write the same service, so the panel survives navigation.
 *
 * Browser-only (it commands and reflects the DOM player); it never renders server-side.
 *
 * Drag-reorder is intentionally absent: the queue engine ({@link PlaylistController}) keeps its order in a
 * private array with no public move/insert, and rebuilding via `setPlaylist` re-clones every entry and drops
 * the currently-playing identity (restarting playback). Reordering therefore needs a new engine method
 * (`moveInPlaylist`) — tracked as a playlist-controller framework change, not worked around here.
 */
@Component({
  selector: 'app-playlist-sidebar',
  imports: [
    RouterLink, BsListGroupComponent, BsListGroupItemComponent, BsProgressComponent, BsProgressBarComponent,
    BsFormComponent, BsFormControlDirective, BsBadgeComponent,
  ],
  template: `
    @if (isBrowser && player.isOpen()) {
      <aside id="play-queue" class="playlist-sidebar bg-body border-start shadow d-flex flex-column"
             [style.margin-top.px]="topOffset()" aria-label="Play queue">
        <header class="d-flex align-items-center gap-2 px-3 py-2 border-bottom">
          <i class="bi bi-music-note-list"></i>
          <span class="fw-semibold flex-grow-1">Queue</span>
          @if (player.queue().length) {
            <bs-badge [type]="Color.secondary">{{ player.queue().length }}</bs-badge>
          }
        </header>

        <!-- Transport -->
        <div class="d-flex align-items-center justify-content-center gap-1 px-3 py-2 border-bottom">
          <button type="button" class="btn btn-sm btn-link p-1 lh-1"
                  [class.text-primary]="player.shuffle()" [class.text-body]="!player.shuffle()"
                  (click)="player.setShuffle(!player.shuffle())"
                  title="Shuffle" aria-label="Shuffle" [attr.aria-pressed]="player.shuffle()">
            <i class="bi bi-shuffle fs-5"></i>
          </button>
          <button type="button" class="btn btn-sm btn-link p-1 lh-1 text-body" (click)="player.previous()"
                  title="Previous" aria-label="Previous">
            <i class="bi bi-skip-start-fill fs-5"></i>
          </button>
          <button type="button" class="btn btn-sm btn-link p-1 lh-1 text-body" (click)="player.togglePlayPause()"
                  [title]="player.isPlaying() ? 'Pause' : 'Play'" [attr.aria-label]="player.isPlaying() ? 'Pause' : 'Play'">
            <i class="bi fs-3" [class.bi-pause-circle-fill]="player.isPlaying()" [class.bi-play-circle-fill]="!player.isPlaying()"></i>
          </button>
          <button type="button" class="btn btn-sm btn-link p-1 lh-1 text-body" (click)="player.next()"
                  title="Next" aria-label="Next">
            <i class="bi bi-skip-end-fill fs-5"></i>
          </button>
          <button type="button" class="btn btn-sm btn-link p-1 lh-1"
                  [class.text-primary]="repeatActive()" [class.text-body]="!repeatActive()"
                  (click)="player.cycleRepeat()"
                  [title]="repeatTitle()" [attr.aria-label]="repeatTitle()">
            <i class="bi fs-5" [class.bi-repeat]="!repeatOne()" [class.bi-repeat-1]="repeatOne()"></i>
          </button>
        </div>

        <!-- Progress -->
        <div class="px-3 py-2 border-bottom">
          <bs-progress [height]="4" ariaLabel="Playback progress">
            <bs-progress-bar [value]="progressValue()" [minimum]="0" [maximum]="progressMax()" />
          </bs-progress>
          <div class="d-flex justify-content-between small text-secondary mt-1">
            <span>{{ formatTime(player.progress()?.currentTime) }}</span>
            <span>{{ formatTime(player.progress()?.duration) }}</span>
          </div>
        </div>

        <!-- Queue -->
        <div class="flex-grow-1 overflow-auto">
          @if (player.queue().length) {
            <bs-list-group class="list-group-flush">
              @for (entry of player.queue(); track entry.key) {
                <bs-list-group-item>
                  <div class="d-flex align-items-center gap-2"
                       [class.fw-semibold]="isCurrent(entry)" [class.text-primary]="isCurrent(entry)">
                    @if (isCurrent(entry)) {
                      <i class="bi bi-volume-up-fill flex-shrink-0"></i>
                    } @else {
                      <i class="bi bi-music-note flex-shrink-0 text-secondary"></i>
                    }
                    @if (entry.routerLink; as link) {
                      <a [routerLink]="link" class="text-truncate flex-grow-1 text-decoration-none text-reset"
                         [title]="player.displayTitle(entry)" (click)="player.closeSidebar()">{{ player.displayTitle(entry) }}</a>
                    } @else {
                      <span class="text-truncate flex-grow-1" [title]="player.displayTitle(entry)">{{ player.displayTitle(entry) }}</span>
                    }
                    <button type="button" class="btn btn-sm btn-link p-0 lh-1 text-secondary flex-shrink-0"
                            (click)="player.remove(entry)" title="Remove" aria-label="Remove from queue">
                      <i class="bi bi-x-lg"></i>
                    </button>
                  </div>
                </bs-list-group-item>
              }
            </bs-list-group>
          } @else {
            <p class="text-secondary text-center p-4 mb-0">The queue is empty.</p>
          }
        </div>

        <!-- Add URL (bs-form ships the .form-control styling for the input) -->
        <bs-form class="border-top d-block" (submitted)="addUrl($event)">
          <div class="d-flex gap-2 p-2">
            <input type="url" class="form-control-sm" placeholder="Add media URL…"
                   [value]="urlInput()" (input)="onUrlInput($event)" aria-label="Media URL" />
            <button type="submit" class="btn btn-sm btn-primary" [disabled]="!urlPlayable()" title="Add to queue">
              <i class="bi bi-plus-lg"></i>
            </button>
          </div>
        </bs-form>
      </aside>
    }
  `,
  styleUrl: './playlist-sidebar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlaylistSidebar {
  protected readonly player = inject(PlayerService);
  private readonly playability = inject(MediaPlayabilityService);
  protected readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  /** Exposed for the `<bs-badge [type]>` binding in the template. */
  protected readonly Color = Color;

  /**
   * Top offset in px (the live topbar height, from the shell's `bsObserveSize`). Applied as the drawer's
   * `margin-top` so it starts below the topbar — keeping the topbar's playlist toggler reachable to close it
   * (which is why the drawer has no close button of its own).
   */
  readonly topOffset = input<number>();

  /** The "Add URL" field value. */
  protected readonly urlInput = signal('');
  /** Whether the typed URL is playable — drives the add button's enabled state. */
  protected readonly urlPlayable = signal(false);

  // ----- repeat button presentation -----
  protected readonly repeatOne = computed(() => this.player.repeat() === ERepeatMode.repeatOne);
  protected readonly repeatActive = computed(() => this.player.repeat() !== ERepeatMode.noRepeat);
  protected readonly repeatTitle = computed(() => {
    switch (this.player.repeat()) {
      case ERepeatMode.repeatOne: return 'Repeat one';
      case ERepeatMode.repeatAll: return 'Repeat all';
      default: return 'Repeat off';
    }
  });

  // ----- progress -----
  protected readonly progressMax = computed(() => this.player.progress()?.duration || 0);
  protected readonly progressValue = computed(() => this.player.progress()?.currentTime ?? 0);

  constructor() {
    effect(() => {
      const url = this.urlInput().trim();
      // Re-check playability as the user types; the async result flips the add button.
      this.playability.canPlay(url).then((playable) => this.urlPlayable.set(playable));
    });
  }

  protected isCurrent(entry: { key: string }): boolean {
    return this.player.currentEntry()?.key === entry.key;
  }

  protected onUrlInput(event: Event): void {
    this.urlInput.set((event.target as HTMLInputElement).value);
  }

  /** Append the typed URL to the queue (only fires when the URL is playable). */
  protected addUrl(event: Event): void {
    event.preventDefault();
    const url = this.urlInput().trim();
    if (!url || !this.urlPlayable()) {
      return;
    }
    this.player.addToQueue([playlistEntryFromUrl(url)]);
    this.urlInput.set('');
    this.urlPlayable.set(false);
  }

  /** Render seconds as `m:ss` (blank when unknown). */
  protected formatTime(seconds: number | null | undefined): string {
    if (seconds == null || !isFinite(seconds)) {
      return '0:00';
    }
    const m = Math.floor(seconds / 60);
    const s = Math.floor(seconds % 60);
    return `${m}:${s.toString().padStart(2, '0')}`;
  }
}
