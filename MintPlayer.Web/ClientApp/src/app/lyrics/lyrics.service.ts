import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

/**
 * Per-recording karaoke timing (mirror of the server `LyricsTiming`): `startTimes[i]` is when lyric line
 * `i` begins, in seconds (`null` = unsynced), parallel to the lines of the lyrics text.
 */
export interface LyricsTiming {
  mediumUrl: string;
  startTimes: (number | null)[];
}

/** A song's lyrics for the karaoke UI (mirror of the server `SongLyricsResult`). */
export interface SongLyrics {
  /** Canonical lyrics text, newline-delimited. Edited via the Spark song form, not here. */
  text: string;
  timings: LyricsTiming[];
  /** Whether the caller may curate timing (Editor / Administrator). */
  canEdit: boolean;
}

/**
 * Reads a song's lyrics (text + karaoke timing) and lets trusted users save timing. The lyrics text is
 * curated through the Spark song edit form (a MultiLineString attribute); only the per-recording timing is
 * written here (`PUT /api/song/lyrics/timings`, Editor/Administrator — cookie auth, sent same-origin).
 */
@Injectable({ providedIn: 'root' })
export class LyricsService {
  private readonly http = inject(HttpClient);

  get(songId: string): Promise<SongLyrics> {
    return firstValueFrom(this.http.get<SongLyrics>('/api/song/lyrics', { params: { id: songId } }));
  }

  saveTimings(songId: string, timings: LyricsTiming[]): Promise<LyricsTiming[]> {
    return firstValueFrom(this.http.put<LyricsTiming[]>('/api/song/lyrics/timings', { songId, timings }));
  }
}
