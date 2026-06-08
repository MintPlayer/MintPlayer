import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { MediaPlayabilityService } from './media-playability.service';
import { PlayerService } from '../player/player.service';
import { playlistEntryFromUrl } from '../player/media-resolver';

/**
 * A play-triangle button shown only when the given media URL is playable by the `<video-player>`
 * (see {@link MediaPlayabilityService}). Clicking it plays the medium in the global, draggable
 * player card by handing it to the {@link PlayerService} queue. Reused by the media column + detail
 * renderers.
 *
 * (Previously opened a self-contained inline overlay; playback is now centralised in the global
 * player card so it persists across navigation and feeds one shared queue — see PRD §4.5.)
 */
@Component({
  selector: 'app-media-play-button',
  template: `
    @if (playable()) {
      <button type="button" class="btn btn-sm btn-link p-0 lh-1 text-success" (click)="play()"
              title="Play" aria-label="Play">
        <i class="bi bi-play-circle-fill fs-5"></i>
      </button>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MediaPlayButton {
  private readonly playability = inject(MediaPlayabilityService);
  private readonly player = inject(PlayerService);

  /** The medium's URL. */
  url = input<string | null>();

  /** Optional display label for the queue entry (defaults to the URL). */
  title = input<string>();

  /** Whether the player can play {@link url} (drives button visibility). */
  readonly playable = signal(false);

  constructor() {
    effect(() => {
      const url = this.url();
      // Re-evaluate playability whenever the URL changes; the async result updates the signal.
      this.playability.canPlay(url).then((playable) => this.playable.set(playable));
    });
  }

  /** Play this medium now in the global player card. */
  protected play(): void {
    const url = this.url();
    if (!url) {
      return;
    }
    this.player.playNow([playlistEntryFromUrl(url, { title: this.title() })]);
  }
}
