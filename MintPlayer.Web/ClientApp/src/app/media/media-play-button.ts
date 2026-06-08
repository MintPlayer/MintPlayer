import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { VideoPlayerComponent } from '@mintplayer/ng-video-player';
import { MediaPlayabilityService } from './media-playability.service';

/**
 * A play-triangle button shown only when the given media URL is playable by the <video-player>
 * (see {@link MediaPlayabilityService}). Clicking it opens a lightweight overlay that plays the
 * medium inline via `<video-player>`. Reused by the media column + detail renderers.
 *
 * Self-contained overlay (no modal-service dependency): a fixed full-screen backdrop with a centered
 * card; click the backdrop or the close button to dismiss (which removes the player and stops playback).
 */
@Component({
  selector: 'app-media-play-button',
  imports: [VideoPlayerComponent],
  template: `
    @if (playable()) {
      <button type="button" class="btn btn-sm btn-link p-0 lh-1 text-success" (click)="open.set(true)"
              [title]="'Play'" aria-label="Play">
        <i class="bi bi-play-circle-fill fs-5"></i>
      </button>
    }

    @if (open()) {
      <div class="mp-player-overlay" (click)="open.set(false)">
        <div class="mp-player-dialog" (click)="$event.stopPropagation()">
          <button type="button" class="btn-close mp-player-close" (click)="open.set(false)" aria-label="Close"></button>
          <video-player [url]="url()!" [autoplay]="true" [width]="800" [height]="450"></video-player>
        </div>
      </div>
    }
  `,
  styles: [`
    .mp-player-overlay {
      position: fixed; inset: 0; z-index: 1080;
      background: rgba(0, 0, 0, .6);
      display: flex; align-items: center; justify-content: center;
    }
    .mp-player-dialog {
      position: relative; background: #000; padding: 0;
      max-width: 95vw; box-shadow: 0 .5rem 1.5rem rgba(0, 0, 0, .5);
    }
    .mp-player-close {
      position: absolute; top: -2rem; right: 0; filter: invert(1);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MediaPlayButton {
  private readonly playability = inject(MediaPlayabilityService);

  /** The medium's URL. */
  url = input<string | null>();

  /** Whether the player can play {@link url} (drives button visibility). */
  readonly playable = signal(false);

  /** Whether the inline player overlay is open. */
  readonly open = signal(false);

  constructor() {
    effect(() => {
      const url = this.url();
      // Re-evaluate playability whenever the URL changes; the async result updates the signal.
      this.playability.canPlay(url).then((playable) => this.playable.set(playable));
    });
  }
}
