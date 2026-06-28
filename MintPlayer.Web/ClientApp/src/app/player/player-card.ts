import { ChangeDetectionStrategy, Component, PLATFORM_ID, computed, inject, signal, viewChild } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CdkDrag, CdkDragEnd, CdkDragHandle } from '@angular/cdk/drag-drop';
import { EPlayerState } from '@mintplayer/player-provider';
import { BsCardComponent, BsCardBodyComponent, BsCardHeaderComponent } from '@mintplayer/ng-bootstrap/card';
import { VideoPlayerComponent } from '@mintplayer/ng-video-player';
import { PlayerService } from './player.service';

/**
 * The global, floating video-player card. A `<bs-card>` made draggable by its header (`cdkDragHandle`),
 * constrained to the viewport, hosting `<video-player>` bound to the {@link PlayerService}. Mounted once
 * in the shell (outside the router-outlet) so playback + drag position survive navigation.
 *
 * Browser-only: drag and the player touch the DOM, so the card never renders server-side. It is shown
 * only while something is loaded ({@link PlayerService.hasCurrent}); when the queue empties the card
 * unmounts, which destroys `<video-player>` and stops playback. Track changes keep the card mounted and
 * just swap `[url]`.
 *
 * Play/pause is two-way with the {@link PlayerService}: `[playerState]` commands the player and
 * `(playerStateChange)` reports back. The service sets `playing` the moment playback starts, so the
 * initial binding agrees with `[autoplay]` rather than pushing an `unstarted` that would fight it.
 * The transport buttons that drive this live on the sidebar (P4).
 *
 * Drag shield: an `<iframe>` (the embedded player) swallows mouse events over its rectangle, so a drag
 * passing over it would stall (the document stops receiving `mousemove`, which cdkDrag relies on). A
 * transparent shield sits above the player inside the card body; it is `pointer-events: none` when idle
 * (so the player's own controls work) and `pointer-events: auto` only while dragging — keeping the moves
 * in the page's DOM. (Mirrors the legacy master-branch fix.)
 */
@Component({
  selector: 'app-player-card',
  imports: [CdkDrag, CdkDragHandle, BsCardComponent, BsCardHeaderComponent, BsCardBodyComponent, VideoPlayerComponent],
  template: `
    @if (isBrowser && player.hasCurrent()) {
      <div class="player-drag-boundary">
        <bs-card
          cdkDrag
          cdkDragBoundary=".player-drag-boundary"
          [cdkDragFreeDragPosition]="dragPosition()"
          (cdkDragStarted)="dragging.set(true)"
          (cdkDragEnded)="onDragEnded($event)"
          class="player-card shadow">
          <bs-card-header cdkDragHandle class="player-handle d-flex align-items-center gap-2">
            <i class="bi bi-grip-vertical text-secondary"></i>
            <span class="text-truncate flex-grow-1" [title]="player.displayTitle(player.currentEntry())">
              {{ player.displayTitle(player.currentEntry()) }}
            </span>
            <button type="button" class="btn btn-sm btn-link p-0 lh-1 text-body" (click)="player.toggleSidebar()"
                    title="Queue" aria-label="Queue">
              <i class="bi bi-list-ul"></i>
            </button>
            <button type="button" class="btn btn-sm btn-link p-0 lh-1 text-body" (click)="player.clear()"
                    title="Close" aria-label="Close">
              <i class="bi bi-x-lg"></i>
            </button>
          </bs-card-header>
          <bs-card-body class="p-0 bg-black position-relative">
            <video-player
              [url]="player.currentEntry()!.url"
              [autoplay]="true"
              [playerState]="player.playerState()"
              [width]="340"
              [height]="191"
              (playerStateChange)="onPlayerState($event)"
              (progressChange)="player.onProgress($event)" />
            <!-- Keeps the drag alive over the iframe; inert (pointer-events:none) when not dragging. -->
            <div class="player-drag-shield" [class.player-drag-shield--active]="dragging()"></div>
          </bs-card-body>
        </bs-card>
      </div>
    }
  `,
  styles: [`
    .player-drag-boundary {
      position: fixed; inset: 0; z-index: 1040; pointer-events: none;
    }
    .player-card {
      position: absolute; right: 1rem; bottom: 1rem; width: 360px;
      pointer-events: auto;
    }
    .player-handle { cursor: move; user-select: none; }
    video-player { display: block; }
    .player-drag-shield {
      position: absolute; inset: 0; z-index: 2; pointer-events: none;
    }
    .player-drag-shield--active { pointer-events: auto; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlayerCard {
  protected readonly player = inject(PlayerService);
  protected readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  /** Restore the remembered drag offset (relative to the card's natural bottom-right spot). */
  protected readonly dragPosition = computed(() => this.player.cardPosition() ?? { x: 0, y: 0 });

  /** True while a drag is in progress — activates the pointer-shield over the iframe. */
  protected readonly dragging = signal(false);

  private readonly videoPlayer = viewChild(VideoPlayerComponent);

  protected onDragEnded(event: CdkDragEnd): void {
    this.dragging.set(false);
    this.player.setCardPosition(event.source.getFreeDragPosition());
  }

  /**
   * Forward player-state to the service and, once the medium is actually playing, ask the player for the
   * real media title and record it for the current entry (the play buttons enqueue with only a URL). By
   * `playing` the provider has loaded metadata, so `getTitle()` resolves to the track name.
   */
  protected onPlayerState(state: EPlayerState): void {
    this.player.onPlayerState(state);
    if (state === EPlayerState.playing) {
      const entry = this.player.currentEntry();
      const player = this.videoPlayer();
      if (entry && player) {
        player.getTitle().then((title) => {
          if (title) {
            this.player.setResolvedTitle(entry.key, title);
          }
        });
      }
    }
  }
}

