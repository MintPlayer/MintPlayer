import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { SparkPoDetailComponent } from '@mintplayer/ng-spark/po-detail';
import { PoContextCapture } from './po-context-capture';
import { PoContextService } from './po-context.service';
import { PlayPlaylistButton } from '../player/play-playlist-button';
import { SubjectLike } from '../subject/subject-like';
import { SongLyrics } from '../lyrics/song-lyrics';

/** CLR types that get app-specific detail-page actions. */
const PLAYLIST_CLR_TYPE = 'MintPlayer.Domain.Entities.Playlist';
const SONG_CLR_TYPE = 'MintPlayer.Domain.Entities.Song';
const SUBJECT_CLR_TYPES: ReadonlySet<string> = new Set([
  'MintPlayer.Domain.Entities.Person',
  'MintPlayer.Domain.Entities.Artist',
  SONG_CLR_TYPE,
]);

/**
 * The app's PersistentObject detail page — the framework's {@link SparkPoDetailComponent} plus app-level
 * additions, registered for every PO via `sparkRoutes({ poDetail })`. It is the single, sanctioned home for
 * per-type detail customisations: the content template publishes the current PO into {@link PoContextService}
 * (zero extra requests — it reuses the framework's already-resolved item + type), and the actions template
 * adds toolbar buttons keyed off that type (today: "Play this playlist"; the subject like widget will follow).
 */
@Component({
  selector: 'app-po-detail',
  imports: [SparkPoDetailComponent, PoContextCapture, PlayPlaylistButton, SubjectLike, SongLyrics],
  template: `
    <spark-po-detail [extraActionsTemplate]="actions" [extraContentTemplate]="capture" />

    <ng-template #actions>
      @if (playlistId(); as id) {
        <app-play-playlist-button [playlistId]="id" />
      }
      @if (subjectId(); as id) {
        <app-subject-like [subjectId]="id" />
      }
    </ng-template>

    <ng-template #capture let-item let-entityType="entityType">
      <app-po-context-capture [item]="item" [entityType]="entityType" />
      @if (songId(); as id) {
        <app-song-lyrics [songId]="id" />
      }
    </ng-template>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppPoDetail {
  private readonly context = inject(PoContextService);

  /** The current PO's id when it is a Playlist, else `null` (drives the "Play this playlist" button). */
  protected readonly playlistId = computed(() => {
    const po = this.context.current();
    return po?.clrType === PLAYLIST_CLR_TYPE ? po.id : null;
  });

  /** The current PO's id when it is a catalog subject, else `null` (drives the like widget). */
  protected readonly subjectId = computed(() => {
    const po = this.context.current();
    return po && SUBJECT_CLR_TYPES.has(po.clrType) ? po.id : null;
  });

  /** The current PO's id when it is a Song, else `null` (drives the lyrics panel). */
  protected readonly songId = computed(() => {
    const po = this.context.current();
    return po?.clrType === SONG_CLR_TYPE ? po.id : null;
  });
}
