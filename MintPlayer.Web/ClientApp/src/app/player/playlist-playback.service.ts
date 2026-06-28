import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { SparkService } from '@mintplayer/ng-spark/services';
import { MediaPlayabilityService } from '../media/media-playability.service';
import { PlaylistEntry } from './playlist-entry';
import { playlistEntryFromUrl } from './media-resolver';

/** One persisted-playlist track as resolved by `GET /api/playlist/playable` (mirror of the server DTO). */
interface PlayablePlaylistTrack {
  songId: string;
  title: string;
  mediaUrls: string[];
}

/** CLR type whose route token deep-links the queue rows back to the song detail page. */
const SONG_CLR_TYPE = 'MintPlayer.Domain.Entities.Song';

/**
 * Turns a persisted {@link Playlist} into the global player's queue. The server resolves each track to its
 * song id, title and candidate media URLs in one round-trip (`/api/playlist/playable`); here we choose the
 * first URL the `<video-player>` can actually play (playability is a client concern — {@link MediaPlayabilityService})
 * and deep-link each entry to its song so the sidebar rows stay clickable (cf. the per-medium play button).
 * Songs with no playable medium are dropped.
 */
@Injectable({ providedIn: 'root' })
export class PlaylistPlaybackService {
  private readonly http = inject(HttpClient);
  private readonly playability = inject(MediaPlayabilityService);
  private readonly spark = inject(SparkService);

  async resolve(playlistId: string): Promise<PlaylistEntry[]> {
    const [tracks, songType] = await Promise.all([
      firstValueFrom(
        this.http.get<PlayablePlaylistTrack[]>('/api/playlist/playable', { params: { id: playlistId } }),
      ),
      this.spark.getEntityTypeByClrType(SONG_CLR_TYPE),
    ]);
    const songToken = songType?.alias || songType?.id;

    const entries: PlaylistEntry[] = [];
    for (const track of tracks) {
      const url = await this.firstPlayable(track.mediaUrls);
      if (!url) {
        continue;
      }
      entries.push(
        playlistEntryFromUrl(url, {
          key: track.songId,
          title: track.title,
          routerLink: songToken ? ['/po', songToken, track.songId] : undefined,
        }),
      );
    }
    return entries;
  }

  private async firstPlayable(urls: readonly string[]): Promise<string | null> {
    for (const url of urls) {
      if (await this.playability.canPlay(url)) {
        return url;
      }
    }
    return null;
  }
}
