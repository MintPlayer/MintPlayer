import { HttpClient } from '@angular/common/http';
import { inject, Injectable, makeStateKey, TransferState } from '@angular/core';
import { catchError, Observable, of } from 'rxjs';
import { PRERENDER_DATA } from '../../ssr/prerender-data';

/** Mirrors MintPlayer.Web.PublicSite.PublicSongDto (camelCased). */
export interface PublicSong {
  /** Numeric part of the RavenDB id ("Songs/313" → "313"), i.e. the URL segment. */
  id: string;
  title: string;
  /** DateOnly as "yyyy-MM-dd", or null. */
  released: string | null;
  artists: { id: string; name: string; credited: boolean }[];
  modifiedAt: string | null;
}

const SONG_STATE = makeStateKey<PublicSong>('public-song');

/**
 * Song data for the public page, from the cheapest source available:
 * 1. server render — the song ASP.NET Core already loaded from RavenDB in OnSupplyData (no HTTP);
 *    it is also written to TransferState so the browser hydrates against identical data;
 * 2. browser, first paint after SSR — read (once) from TransferState;
 * 3. browser, client-side navigation — GET /api/public/song/{id}.
 */
@Injectable({ providedIn: 'root' })
export class PublicSongService {
  private readonly http = inject(HttpClient);
  private readonly transferState = inject(TransferState);
  private readonly prerenderData = inject(PRERENDER_DATA);

  get(id: string): Observable<PublicSong | null> {
    const supplied = this.prerenderData?.['song'] as PublicSong | undefined;
    if (supplied && supplied.id === id) {
      this.transferState.set(SONG_STATE, supplied);
      return of(supplied);
    }
    if (this.prerenderData) {
      // Server render but OnSupplyData found nothing (the response is already a 404).
      return of(null);
    }

    const transferred = this.transferState.get(SONG_STATE, null);
    if (transferred && transferred.id === id) {
      this.transferState.remove(SONG_STATE);
      return of(transferred);
    }

    return this.http.get<PublicSong>(`/api/public/song/${encodeURIComponent(id)}`).pipe(catchError(() => of(null)));
  }
}
