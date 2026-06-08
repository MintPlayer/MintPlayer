import { ChangeDetectionStrategy, Component, PLATFORM_ID, computed, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CdkDrag, CdkDragEnd, CdkDragHandle } from '@angular/cdk/drag-drop';
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
 * Play/pause commanding is deliberately not wired here (the `[playerState]` input would push an initial
 * `unstarted` that fights `[autoplay]`); transport controls live on the sidebar (P4).
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
          (cdkDragEnded)="onDragEnded($event)"
          class="player-card shadow">
          <bs-card-header cdkDragHandle class="player-handle d-flex align-items-center gap-2">
            <i class="bi bi-grip-vertical text-secondary"></i>
            <span class="text-truncate flex-grow-1" [title]="player.currentEntry()?.title">
              {{ player.currentEntry()?.title }}
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
          <bs-card-body class="p-0 bg-black">
            <video-player
              [url]="player.currentEntry()!.url"
              [autoplay]="true"
              [width]="340"
              [height]="191"
              (playerStateChange)="player.onPlayerState($event)"
              (progressChange)="player.onProgress($event)" />
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
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlayerCard {
  protected readonly player = inject(PlayerService);
  protected readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  /** Restore the remembered drag offset (relative to the card's natural bottom-right spot). */
  protected readonly dragPosition = computed(() => this.player.cardPosition() ?? { x: 0, y: 0 });

  protected onDragEnded(event: CdkDragEnd): void {
    this.player.setCardPosition(event.source.getFreeDragPosition());
  }
}
