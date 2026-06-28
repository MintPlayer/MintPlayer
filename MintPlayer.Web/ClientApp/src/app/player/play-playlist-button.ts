import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { NotificationKind, SparkNotificationService } from '@mintplayer/ng-spark/client-operations';
import { PlayerService } from './player.service';
import { PlaylistPlaybackService } from './playlist-playback.service';
import { PlaylistEntry } from './playlist-entry';

/**
 * The "Play this playlist" toolbar action on the Playlist detail page (mounted by {@link AppPoDetail}).
 * Offers the legacy app's three modes — Play (replace the queue), Shuffle (replace + shuffle), and Add to
 * queue (append) — over the one shared {@link PlayerService}. Resolution goes through
 * {@link PlaylistPlaybackService}; an empty/failed resolve surfaces a toast rather than silently doing nothing.
 */
@Component({
  selector: 'app-play-playlist-button',
  template: `
    <div class="btn-group" role="group" aria-label="Play this playlist">
      <button type="button" class="btn btn-outline-primary" (click)="play(false)" [disabled]="loading()"
              title="Play this playlist">
        @if (loading()) {
          <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
        } @else {
          <i class="bi bi-play-fill"></i>
        }
        Play
      </button>
      <button type="button" class="btn btn-outline-primary" (click)="play(true)" [disabled]="loading()"
              title="Shuffle play" aria-label="Shuffle play">
        <i class="bi bi-shuffle"></i>
      </button>
      <button type="button" class="btn btn-outline-primary" (click)="enqueue()" [disabled]="loading()"
              title="Add to queue" aria-label="Add to queue">
        <i class="bi bi-plus-lg"></i>
      </button>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlayPlaylistButton {
  private readonly playback = inject(PlaylistPlaybackService);
  private readonly player = inject(PlayerService);
  private readonly notifications = inject(SparkNotificationService);

  /** Id of the playlist being shown (e.g. `"Playlists/5"`). */
  readonly playlistId = input.required<string>();

  protected readonly loading = signal(false);

  protected async play(shuffle: boolean): Promise<void> {
    const entries = await this.load();
    if (!entries) {
      return;
    }
    this.player.setShuffle(shuffle);
    this.player.playNow(entries);
    this.player.setSidebarOpen(true);
  }

  protected async enqueue(): Promise<void> {
    const entries = await this.load();
    if (!entries) {
      return;
    }
    this.player.addToQueue(entries);
    this.player.setSidebarOpen(true);
  }

  /** Resolve the playlist's playable entries, guarding against double-clicks and reporting empties/errors. */
  private async load(): Promise<PlaylistEntry[] | null> {
    if (this.loading()) {
      return null;
    }
    this.loading.set(true);
    try {
      const entries = await this.playback.resolve(this.playlistId());
      if (entries.length === 0) {
        this.notifications.show('This playlist has no playable tracks.', NotificationKind.Warning);
        return null;
      }
      return entries;
    } catch {
      this.notifications.show('Could not load this playlist.', NotificationKind.Error);
      return null;
    } finally {
      this.loading.set(false);
    }
  }
}
