import { Injectable, computed, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { EPlayerState } from '@mintplayer/player-provider';
import { PlayerProgress } from '@mintplayer/player-progress';
import { ERepeatMode, PlaylistController } from '@mintplayer/playlist-controller';
import { PlaylistEntry } from './playlist-entry';

/** Imperative operations on the mounted player that have no `<video-player>` input binding. */
export interface PlayerHandle {
  seek(positionSec: number): void;
}

/** The floating player card's free-drag position, in px relative to its natural spot. */
export interface CardPosition {
  x: number;
  y: number;
}

/**
 * The single source of truth for playback — a root singleton wrapping the `@mintplayer/playlist-controller`
 * queue engine and exposing its state as signals (the app's idiom; cf. `MediaPlayabilityService`). The
 * player card, the playlist sidebar, and the per-row play buttons all read/mutate this one service, so
 * playback + queue survive navigation (unlike the legacy app, which `new`-ed the controller inside its
 * AppComponent).
 *
 * Engine contract (proven in P0.2): `addToPlaylist`/`setPlaylist` clone entries and the controller matches
 * on object identity of its own clones. Therefore {@link queue} exposes the controller's own instances and
 * {@link remove} must be passed one of them (the sidebar does this by iterating `queue()`).
 */
@Injectable({ providedIn: 'root' })
export class PlayerService {
  private readonly controller = new PlaylistController<PlaylistEntry>();

  /** The entry currently loaded in the player (`null` = nothing playing). Bridged from the engine's `video$`. */
  readonly currentEntry = toSignal(this.controller.video$, { initialValue: null });

  private readonly _queue = signal<readonly PlaylistEntry[]>([]);
  /** The live queue — the controller's own entry instances (pass these back to {@link remove}). */
  readonly queue = this._queue.asReadonly();

  private readonly _playerState = signal<EPlayerState>(EPlayerState.unstarted);
  /** Two-way with `<video-player>`: drives `[playerState]`, and {@link onPlayerState} writes it back. */
  readonly playerState = this._playerState.asReadonly();

  private readonly _progress = signal<PlayerProgress | null>(null);
  readonly progress = this._progress.asReadonly();

  private readonly _shuffle = signal(false);
  readonly shuffle = this._shuffle.asReadonly();

  private readonly _repeat = signal<ERepeatMode>(ERepeatMode.noRepeat);
  readonly repeat = this._repeat.asReadonly();

  private readonly _isOpen = signal(false);
  /** Whether the playlist sidebar is open. */
  readonly isOpen = this._isOpen.asReadonly();

  private readonly _cardPosition = signal<CardPosition | null>(null);
  /** The floating card's drag position, remembered for the session. */
  readonly cardPosition = this._cardPosition.asReadonly();

  private readonly _resolvedTitles = signal<ReadonlyMap<string, string>>(new Map());
  /**
   * Real media titles resolved from the player (`<video-player>.getTitle()`), keyed by {@link PlaylistEntry.key}.
   * Overlaid on the queue display so entries enqueued with only a URL (the play buttons can't know the track
   * name) show the actual title once it loads — see {@link displayTitle}. Kept separate from the immutable,
   * engine-cloned entries rather than mutating them.
   */
  readonly resolvedTitles = this._resolvedTitles.asReadonly();

  private readonly _muted = signal(false);
  /** Bound to `<video-player [mute]>`. */
  readonly muted = this._muted.asReadonly();

  /** `performance.now()` at the last {@link onProgress} — lets a caller extrapolate the live position. */
  private progressAt = 0;

  /** Imperative bridge to the mounted `<video-player>` (seek has no input binding). Set by the player card. */
  private handle: PlayerHandle | null = null;

  /** True when something is loaded — drives the player card's visibility. */
  readonly hasCurrent = computed(() => this.currentEntry() !== null);

  /** True while the current entry is actively playing. */
  readonly isPlaying = computed(() => this.playerState() === EPlayerState.playing);

  // ----- queue mutations -----

  /** Replace the queue with `entries` and start playing the first. */
  playNow(entries: PlaylistEntry[]): void {
    // setPlaylist is async but has no awaits — its body (and the video$ emission) runs synchronously.
    void this.controller.setPlaylist(entries);
    this.syncQueue();
    this._progress.set(null); // the previous medium's position no longer applies
    // Command `playing` optimistically: the card binds `[playerState]`, and starting from `playing`
    // (rather than the initial `unstarted`) makes that binding agree with `[autoplay]` instead of
    // pushing an `unstarted` that fights it. The player's own `playerStateChange` then confirms it.
    this._playerState.set(EPlayerState.playing);
  }

  /** Append `entries`; starts playing if nothing is currently loaded. */
  addToQueue(entries: PlaylistEntry[]): void {
    const wasEmpty = !this.hasCurrent();
    this.controller.addToPlaylist(...entries);
    this.syncQueue();
    if (wasEmpty) {
      this._playerState.set(EPlayerState.playing); // see playNow — keep the binding aligned with autoplay.
    }
  }

  /** Remove an entry. Pass an instance obtained from {@link queue} (identity-matched by the engine). */
  remove(entry: PlaylistEntry): void {
    this.controller.removeFromPlaylist(entry);
    this.syncQueue();
  }

  /** Empty the queue and stop playback (current → null, which hides the player card). */
  clear(): void {
    void this.controller.setPlaylist([]);
    this.syncQueue();
  }

  // ----- transport -----

  next(): void {
    this.controller.next();
  }

  previous(): void {
    this.controller.previous();
  }

  /** Toggle play/pause by commanding `<video-player>` through the {@link playerState} signal. */
  togglePlayPause(): void {
    this._playerState.set(this.isPlaying() ? EPlayerState.paused : EPlayerState.playing);
  }

  /** Command play / pause explicitly (listen-together follows the room state rather than toggling). */
  setPlaying(playing: boolean): void {
    this._playerState.set(playing ? EPlayerState.playing : EPlayerState.paused);
  }

  /** Seek the current medium. Returns false when no player is mounted yet. */
  seek(positionSec: number): boolean {
    if (!this.handle) {
      return false;
    }
    this.handle.seek(positionSec);
    return true;
  }

  setMuted(muted: boolean): void {
    this._muted.set(muted);
  }

  /**
   * Best estimate of the medium's position right now: the last reported `currentTime` extrapolated by the
   * wall-clock time since (when playing). `null` before the first progress report of the current medium.
   */
  livePosition(): number | null {
    const progress = this._progress();
    if (!progress) {
      return null;
    }
    const elapsed = this.isPlaying() ? (performance.now() - this.progressAt) / 1000 : 0;
    return progress.currentTime + elapsed;
  }

  /** Called by the player card when `<video-player>` mounts / unmounts. */
  registerPlayer(handle: PlayerHandle | null): void {
    this.handle = handle;
    if (!handle) {
      this._progress.set(null);
    }
  }

  setShuffle(value: boolean): void {
    this.controller.shuffle = value;
    this._shuffle.set(value);
  }

  /** Cycle noRepeat → repeatOne → repeatAll → noRepeat. */
  cycleRepeat(): void {
    const order = [ERepeatMode.noRepeat, ERepeatMode.repeatOne, ERepeatMode.repeatAll];
    const next = order[(order.indexOf(this._repeat()) + 1) % order.length];
    this.controller.repeat = next;
    this._repeat.set(next);
  }

  // ----- sidebar + card -----

  toggleSidebar(): void {
    this._isOpen.update((open) => !open);
  }

  closeSidebar(): void {
    this._isOpen.set(false);
  }

  setSidebarOpen(open: boolean): void {
    this._isOpen.set(open);
  }

  setCardPosition(position: CardPosition): void {
    this._cardPosition.set(position);
  }

  /** Record a player-resolved media title for an entry (no-op for blank/unchanged values). */
  setResolvedTitle(key: string, title: string): void {
    const trimmed = title.trim();
    if (!trimmed) {
      return;
    }
    this._resolvedTitles.update((map) => {
      if (map.get(key) === trimmed) {
        return map;
      }
      const next = new Map(map);
      next.set(key, trimmed);
      return next;
    });
  }

  /** Display label for an entry: the player-resolved title if known, else the entry's own placeholder. */
  displayTitle(entry: PlaylistEntry | null | undefined): string {
    if (!entry) {
      return '';
    }
    return this._resolvedTitles().get(entry.key) ?? entry.title;
  }

  // ----- bindings from the <video-player> outputs -----

  /** Fed by `(playerStateChange)`. Auto-advances the queue on `ended`. */
  onPlayerState(state: EPlayerState): void {
    this._playerState.set(state);
    if (state === EPlayerState.ended) {
      this.controller.playerEnded();
    }
  }

  /** Fed by `(progressChange)`. Also feeds the engine's previous() restart-vs-back heuristic. */
  onProgress(progress: PlayerProgress): void {
    this._progress.set(progress);
    this.progressAt = performance.now();
    this.controller.currentVideoPosition = progress.currentTime;
  }

  /** Snapshot the controller's queue into the signal (the engine mutates its array in place). */
  private syncQueue(): void {
    this._queue.set(this.controller.playlist);
  }
}
