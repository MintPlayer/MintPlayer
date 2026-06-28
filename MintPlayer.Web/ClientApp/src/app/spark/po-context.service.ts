import { Injectable, signal } from '@angular/core';

/** The currently-displayed PersistentObject on a detail page. */
export interface PoContext {
  /** RavenDB document id, e.g. `"Playlists/5"`. */
  readonly id: string;
  /** Stable CLR type name (e.g. `"MintPlayer.Domain.Entities.Playlist"`) — the key for per-type behaviour. */
  readonly clrType: string;
  /** The route token the PO is reached under (`alias || type id`) — for building deep-links to it. */
  readonly routeType: string;
}

/**
 * Tracks which PersistentObject the {@link AppPoDetail} wrapper is currently showing, so app-level
 * additions to the detail page (the "Play this playlist" toolbar button, and the like widget later) can
 * react to the PO's type and id without re-fetching metadata. Populated by {@link PoContextCapture} from
 * the framework's own resolved item + entity type — no extra HTTP. There is only ever one detail page
 * mounted, hence a single root signal rather than a per-instance value.
 */
@Injectable({ providedIn: 'root' })
export class PoContextService {
  private readonly _current = signal<PoContext | null>(null);

  /** The PO on the active detail page, or `null` when none is shown. */
  readonly current = this._current.asReadonly();

  set(context: PoContext): void {
    this._current.set(context);
  }

  clear(): void {
    this._current.set(null);
  }
}
