# PRD — Draggable Media Player + Play-Queue + Playlist Sidebar

**Status:** In implementation — P0–P4 done (deps, `PlayerService`, draggable card, play-button wiring, **docked sidebar + play/pause commanding**), browser-verified end-to-end. Remaining: P5 polish (display-name through play buttons, prerender smoke, sidebar/e2e specs) and a framework follow-up for queue **reorder** (see D6). Live status in the [companion plan](./Implementation-Plan-Player-Playlist.md).
**Author:** Pieterjan De Clippel (with Claude)
**Date:** 2026-06-08
**Parent docs:** [`PRD-Spark-Migration.md`](./PRD-Spark-Migration.md) · [`Implementation-Plan-Spark-Migration.md`](./Implementation-Plan-Spark-Migration.md)
**Companion plan:** [`Implementation-Plan-Player-Playlist.md`](./Implementation-Plan-Player-Playlist.md)

---

## 1. Summary

The legacy MintPlayer had a **global media player** (a floating card hosting `<video-player>`), an in-memory **play-queue** (the `PlaylistController` queue engine: shuffle / repeat / prev / next / auto-advance), and a docked **playlist-sidebar** showing the queue. On the Spark branch only a per-row inline play button exists today (`MediaPlayButton`, Phase 2.7); there is no shared player, no queue, and no sidebar.

This PRD reintroduces that experience on the Spark branch in the current Angular-22 / signals idiom:

1. A **draggable player card** — `<video-player>` inside a `<bs-card cdkDrag>`, rendered once in the `Shell` so playback survives navigation.
2. A **play-queue controller** — the queue/shuffle/repeat/auto-advance logic, exposed as a **root signal-based `PlayerService`** (the single source of truth).
3. A **playlist-sidebar** — a docked panel rendering the live queue with transport controls, drag-reorder, and remove.

### Answering the explicit question — *"Do we need some form of state management here?"*

**Yes — a single root-provided, signal-based `PlayerService` is the correct and necessary choice**, for three reasons:

- **Multiple, navigation-surviving consumers.** The player card, the sidebar, the per-row play buttons on song/artist/person pages, and a future "Play" action on saved playlists all read and mutate *one* queue. That state must outlive any route component, so it cannot live in a page component (the legacy app's core structural weakness — it `new`-ed the controller inside `AppComponent`).
- **It matches the established idiom.** The Spark app already standardises on `providedIn: 'root'` services exposing **signals** + `inject()` (e.g. `MediaPlayabilityService`). A root `PlayerService` exposing `currentEntry`, `queue`, `playerState`, `progress`, `shuffle`, `repeat` as signals is the idiomatic fit — no NgRx, no component-owned `BehaviorSubject`s.
- **We don't reinvent the hard part.** The non-trivial prev/next/shuffle/repeat/history algorithm already exists, battle-tested, in the **`@mintplayer/playlist-controller`** package (sibling of `ng-video-player`, used by its own demo). `PlayerService` *wraps* that engine and **bridges its RxJS `video$` to a signal** (`toSignal`), so we get correct queue semantics while presenting a clean signal surface to the rest of the app.

This is deliberately **not** NgRx/component-store: the state is small, single-instance, and already has a domain engine. A root service with signals is the lowest-ceremony option that satisfies all consumers.

---

## 2. Goals & Non-Goals

### 2.1 Goals

| # | Goal |
|---|------|
| G1 | A floating, **draggable** player card (`<bs-card cdkDrag>` + `cdkDragHandle`) hosting `<video-player>`, constrained to the viewport, position remembered for the session. |
| G2 | A **root `PlayerService`** (signals) owning the queue + transport, wrapping `@mintplayer/playlist-controller`; auto-advance on `EPlayerState.ended`. |
| G3 | A **playlist-sidebar** showing the live queue: current-track highlight, transport (prev / play-pause / next / shuffle / repeat), progress, per-item remove, drag-reorder, "add URL", open/close toggle. |
| G4 | Per-row **play buttons** (existing media renderers) route through `PlayerService` (`playNow` / `addToQueue`) instead of the isolated inline overlay. |
| G5 | Stay strictly in the app's coding style: standalone + `OnPush`, `input()`/`signal()`/`effect()`, `inject()`, `@if`/`@for`, hyphenated filenames, SSR-safe. |
| G6 | Reusable building blocks (the card, the sidebar, the service) factored so a future saved-`Playlist` "Play" action can feed the same queue. |

### 2.2 Non-Goals

| # | Non-goal |
|---|----------|
| N1 | **Server-persisted `Playlist` entity + CRUD.** That remains **Phase 3.1** of the master plan. This PRD is the *client-side playback experience*; `PlayerService.setQueue(...)` is designed to later accept a loaded playlist's tracks, but no RavenDB `Playlist` collection, controller, or "save queue as playlist" is built here. |
| N2 | Cross-session queue persistence (the queue lives for the browser session; legacy didn't persist it either). |
| N3 | Karaoke/lyrics sync (legacy coupled lyrics to player progress — that is Phase 3.2; out of scope, but `progress` signal is exposed so 3.2 can consume it). |
| N4 | Reordering the *persisted* playlist (no persisted playlist here). Sidebar reorder mutates the in-memory queue only. |
| N5 | Mobile-specific player chrome / PiP polish beyond what `<video-player>` already provides. |

---

## 3. Current state (from the investigation)

- **Client app:** `C:\Repos\MintPlayer\MintPlayer.Web\ClientApp` (Angular 22, standalone, signals).
- **Player today:** `src/app/media/media-play-button.ts` — a play button that opens a **self-contained fixed overlay** with `<video-player [url] [autoplay]>`; gated by `MediaPlayabilityService.canPlay()` (delegates to `@mintplayer/video-player` `findApis`). Bound into Spark via the `media-player` renderer on `Medium.Value` (`renderers/media-column-renderer.ts`, `media-detail-renderer.ts`). **No shared state, no queue.**
- **Plugins:** `src/app/media/video-player-plugins.ts` — `VIDEO_PLAYER_PLUGINS` already registered via `provideVideoApis(...)`; reuse verbatim.
- **Shell:** `src/app/shell/shell.ts` + `shell.html` — `<bs-shell>` with topbar (`bsShellTopbar`) + program-unit sidebar (`bsShellSidebar`) + `<router-outlet>` in `<main>`. The player card and the new sidebar mount here so they persist across navigation.
- **Idiom:** `signal()` / `input()` / `effect()` / `inject()`; `OnPush`; `@if`/`@for`; inline templates for small components; hyphenated filenames **without** `.component` suffix; classes PascalCase **without** `Component` suffix; SSR guarded via `afterNextRender()` / `isPlatformBrowser`.

### Library APIs we build on (verbatim)

- **`<bs-card>`** from `@mintplayer/ng-bootstrap/card`: standalone slot components `BsCardComponent`/`BsCardHeaderComponent`/`BsCardBodyComponent`/`BsCardFooterComponent` (+ `BsListGroupComponent`/`BsListGroupItemComponent` for the queue). No outputs — containers only. Host is `display:block`; give it `position:fixed` for floating.
- **`cdkDrag`** from `@angular/cdk/drag-drop`: `cdkDrag` (free 2D drag via `transform`), `cdkDragHandle`, `cdkDragBoundary="<selector>"`, `[cdkDragFreeDragPosition]`, `(cdkDragEnded)` → `event.source.getFreeDragPosition()` to persist. For the sidebar list: `cdkDropList` + `(cdkDropListDropped)` + `moveItemInArray`. **`@angular/cdk@22.0.0` is present transitively (via ng-bootstrap) — add it as a direct dependency before use.**
- **`<video-player>`** from `@mintplayer/ng-video-player`: input `[url]` (+ `[autoplay] [width] [height] [playerState] [volume] [mute]`), method `setUrl(url|null)`, outputs `(playerStateChange: EPlayerState)` and `(progressChange: PlayerProgress)`. **No dedicated `ended` output** — "ended" arrives as `playerStateChange === EPlayerState.ended` (`EPlayerState` from `@mintplayer/player-provider`: `unstarted=1, playing=2, paused=3, ended=4`).
- **`PlaylistController<TVideo>`** from `@mintplayer/playlist-controller`: `video$: BehaviorSubject<TVideo|null>`, `get playlist(): TVideo[]`, `shuffle`, `repeat: ERepeatMode` (`noRepeat|repeatOne|repeatAll`), `currentVideoPosition`, `addToPlaylist(...v)`, `setPlaylist(v[])`, `removeFromPlaylist(v)`, `previous()`, `next()`, `playerEnded()`. **Add as a direct dependency** (not currently installed; the legacy app used the older `@mintplayer/ng-playlist-controller`).

---

## 4. Design

### 4.1 The queue item model

```ts
// src/app/player/playlist-entry.ts
export interface PlaylistEntry {
  /** Stable key for track-by + dedupe (song id, or the url for ad-hoc entries). */
  readonly key: string;
  /** The media URL handed to <video-player>. Must be findApis-playable. */
  readonly url: string;
  /** Display label in the sidebar (song title / breadcrumb, or the raw url). */
  readonly title: string;
  /** Optional deep-link to the catalog detail page (songs); absent for ad-hoc urls. */
  readonly routerLink?: string[];
}
```

Mirrors the legacy `SongWithMedium | VideoUrl` split but as one flat, display-ready shape (resolution of "which medium to play" happens at enqueue time, not in the player). `TVideo = PlaylistEntry`.

### 4.2 `PlayerService` (root, signals) — the source of truth

```ts
@Injectable({ providedIn: 'root' })
export class PlayerService {
  // wraps a single PlaylistController<PlaylistEntry> instance
  readonly currentEntry: Signal<PlaylistEntry | null>;   // bridged from controller.video$ via toSignal
  readonly queue:        Signal<readonly PlaylistEntry[]>;
  readonly playerState:  Signal<EPlayerState>;           // writable signal, fed by the card
  readonly progress:     Signal<PlayerProgress | null>;
  readonly shuffle:      Signal<boolean>;
  readonly repeat:       Signal<ERepeatMode>;
  readonly isOpen:       Signal<boolean>;                // sidebar open/closed
  readonly hasCurrent:   Signal<boolean>;                // → show/hide the card

  playNow(entries: PlaylistEntry[]): void;     // setPlaylist
  addToQueue(entries: PlaylistEntry[]): void;  // addToPlaylist
  remove(entry: PlaylistEntry): void;
  jumpTo(entry: PlaylistEntry): void;          // play a specific queued entry
  next(): void; previous(): void;
  togglePlayPause(): void;
  setShuffle(v: boolean): void; cycleRepeat(): void;
  toggleSidebar(): void;

  // called by the player card's (output) bindings:
  onPlayerState(state: EPlayerState): void;    // ended → controller.playerEnded()
  onProgress(p: PlayerProgress): void;         // → controller.currentVideoPosition + progress signal
}
```

- Internally holds `new PlaylistController<PlaylistEntry>()` and bridges `video$` → `currentEntry` signal with `toSignal`. `queue` is recomputed from `controller.playlist` on each `video$` emission (the controller mutates the array in place, so we snapshot/copy).
- **⚠ Identity contract (proven in P0.2):** `addToPlaylist`/`setPlaylist` **clone** each entry (`Object.assign`), and the controller's `removeFromPlaylist`/navigation match on **object identity of its own clones** — *not* on the originals you enqueued. Therefore `PlayerService` must expose `queue()` as **the controller's own instances** (`controller.playlist`, which already returns them) and `remove(entry)`/`jumpTo(entry)` must receive an instance taken from `queue()`. The sidebar naturally does this (it `@for`s over `player.queue()` and passes those back). Display/track-by uses `entry.key`; the controller calls use the same object reference.
- `shuffle`/`repeat` are plain fields on the controller; `PlayerService` mirrors them into signals and writes through on the setters (keeps two-way UI binding signal-clean).
- **Auto-advance:** the card binds `(playerStateChange)="player.onPlayerState($event)"`; on `ended` the service calls `controller.playerEnded()`, which emits the next `video$`, which the card observes and `setUrl`s. Identical data-flow to the legacy `AppComponent`, but centralised in the service.

### 4.3 `PlayerCard` — floating draggable card (mounted in `Shell`)

```html
<!-- shown only when player.hasCurrent() -->
<div class="player-drag-boundary"><!-- fixed, inset:0, pointer-events:none -->
  <bs-card cdkDrag cdkDragBoundary=".player-drag-boundary"
           [cdkDragFreeDragPosition]="player position signal"
           (cdkDragEnded)="persistPosition($event)"
           class="player-card">                     <!-- position:fixed; pointer-events:auto -->
    <bs-card-header cdkDragHandle class="player-handle">  <!-- cursor:move -->
      {{ player.currentEntry()?.title }}
      <button (click)="player.toggleSidebar()">…queue…</button>
    </bs-card-header>
    <bs-card-body class="p-0 position-relative">
      <video-player [url]="player.currentEntry()!.url"
                    [autoplay]="true"
                    (playerStateChange)="player.onPlayerState($event)"
                    (progressChange)="player.onProgress($event)" />
      <!-- Drag-shield: see below. Inert when idle, active while dragging. -->
      <div class="player-drag-shield" [class.player-drag-shield--active]="dragging()"></div>
    </bs-card-body>
  </bs-card>
</div>
```

- `cdkDragHandle` on the header means buttons/iframe stay interactive; only the header drags.
- **Drag-shield over the iframe (D11).** An `<iframe>` (the embedded player) swallows mouse events over its rectangle, so a drag passing over it stalls (the document stops receiving `mousemove`, which cdkDrag relies on). A transparent shield sits above the player inside the card body: `pointer-events: none` when idle (the player's own controls work), toggled to `pointer-events: auto` only while dragging (`(cdkDragStarted)`→`dragging=true`, `(cdkDragEnded)`→`false`) so the moves stay in the page's DOM. Mirrors the legacy master-branch fix. (Implemented P2; verified live.)
- Position persists in a `PlayerService` signal for the session; restored via `[cdkDragFreeDragPosition]`. **SSR-guarded** — the card renders only in the browser (`@if (isBrowser && player.hasCurrent())`).
- The card unmounts when the queue empties (`hasCurrent()` false), which destroys `<video-player>` and stops playback; track changes keep it mounted and swap `[url]`. So `[url]` is only ever bound to a non-null URL.
- Mounted in `shell.html` after `</bs-shell>` (outside `<router-outlet>`) so it never unmounts on navigation.

### 4.4 `PlaylistSidebar` — docked queue panel (mounted in `Shell`)

- Driven entirely by `PlayerService` signals (a "smart" component here, since the service is the store — simpler than the legacy dumb-component + giant `AppComponent` wiring).
- Transport bar: shuffle toggle, repeat cycle (`noRepeat→repeatOne→repeatAll`), previous, play/pause, next.
- Progress row from `player.progress()`.
- `<bs-list-group>` of `@for (entry of player.queue(); track entry.key)`: title, now-playing highlight when `entry.key === player.currentEntry()?.key`, `routerLink` for songs / external link for urls, a remove button. **Drag-reorder is deferred (see D6)** — the queue engine has no in-place move primitive, so it ships without reorder until a framework method lands.
- "Add URL" button → small input/modal → `player.addToQueue([{ key:url, url, title:url }])` (gated by `MediaPlayabilityService.canPlay`).
- Open/close bound to `player.isOpen()`; a toggle lives in the player-card header and/or the shell topbar.

### 4.5 Integration with existing play buttons

`MediaPlayButton` (and the two media renderers) currently open a private overlay. **Decision (D7):** the play button instead calls `player.playNow([entry])` (or `addToQueue`) so all playback flows through the global card. The standalone inline overlay is retired (its job is now the global card). The renderers keep their playability gating via `MediaPlayabilityService`.

---

## 5. Decisions

| # | Decision | Choice & rationale |
|---|----------|--------------------|
| D1 | Reuse vs. reimplement the queue engine | **Reuse `@mintplayer/playlist-controller`.** Correct prev/next/shuffle/repeat/history already exists and is demoed against `ng-video-player`. We wrap, not rewrite. |
| D2 | State management | **Root `providedIn:'root'` `PlayerService` exposing signals**, bridging the controller's `video$` via `toSignal`. No NgRx. Matches `MediaPlayabilityService`. |
| D3 | Where the player mounts | **In `Shell`**, outside `<router-outlet>`, so playback + queue survive navigation. |
| D4 | Draggable mechanism | **`cdkDrag` on `<bs-card>` host + `cdkDragHandle` on header + `cdkDragBoundary` to a fixed full-viewport layer**; position persisted in a service signal, restored via `[cdkDragFreeDragPosition]`. |
| D5 | New dependencies | Add **`@angular/cdk@^22`** (currently only transitive) and **`@mintplayer/playlist-controller@^20`** as direct deps. *(Corrected during P0: the queue engine is the framework-agnostic **core** package, versioned in the `20.x` line alongside `@mintplayer/video-player@20` / `player-provider@20` / the `@20` plugins already in package.json — not `22`. Peer dep `rxjs ^7.4.0`, satisfied.)* |
| D6 | Sidebar reorder | **Deferred — needs a framework change (revised during P4).** The plan was `cdkDropList` + `moveItemInArray` → `controller.setPlaylist`, but `setPlaylist` re-clones every entry and drops the currently-playing identity (restarts playback), and the engine keeps order in a private `_playlist` with no public move/insert. Clean reorder requires a new `moveInPlaylist(from, to)` on `@mintplayer/playlist-controller` (batched framework change, P5.5). The sidebar ships without reorder; everything else (transport/remove/add/now-playing) is independent of it. |
| D7 | Existing play buttons | **Route through `PlayerService`**; retire the per-button inline overlay. |
| D8 | Persisted `Playlist` entity | **Out of scope — stays Phase 3.1.** `PlayerService` is shaped to accept a saved playlist's tracks later. |
| D9 | "Ended" detection | Via `(playerStateChange) === EPlayerState.ended` (no dedicated output). |
| D10 | SSR | Player card + drag are **browser-only** (`isPlatformBrowser` / `afterNextRender`); the service is SSR-inert until a play action occurs. |
| D11 | Iframe swallows drag events | A transparent **drag-shield** over the player, `pointer-events: none` when idle (native controls work) and `auto` only while dragging, keeps `mousemove` in the page DOM so the drag doesn't stall over the iframe. (Legacy master-branch fix; implemented P2.) |
| D12 | Play/pause commanding without fighting autoplay | The card binds `[playerState]="player.playerState()"` (the only public command surface on `<video-player>` is the `playerState` setter), and `PlayerService` sets `playing` the instant playback starts (`playNow` / `addToQueue`-from-empty). So the initial binding pushes `playing` (agreeing with `[autoplay]`) instead of the default `unstarted` that would fight it; `(playerStateChange)` then writes the real state back. Toggle round-trip is idempotent (no feedback loop). Resolves the play/pause deferral noted in P2. |

---

## 6. Risks & open questions

| # | Risk / question | Mitigation |
|---|------------------|------------|
| R1 | Version skew: `@mintplayer/playlist-controller` must align with the installed player core. | **Resolved in P0:** use **`^20.0.0`** — the queue engine is part of the `@20` framework-agnostic core family (`video-player`/`player-provider`/plugins), not the `22.x` Angular wrapper line. Smoke-test enqueue→play→ended→advance before building UI. |
| R2 | A `Subject` can have several media; "which to play?" | Enqueue-time resolution: pick the first `MediaPlayabilityService.canPlay`-true medium (prefer `Visible`). Keep the rule in one helper. |
| R3 | `cdkDrag` relies on a transitive `@angular/cdk`. | Promote to a direct dependency (D5) so it can't vanish on a bootstrap bump. |
| R4 | SSR: drag/position touches `window`. | Browser-only rendering + `afterNextRender`; nothing drag-related runs server-side. |
| R5 | "PlaylistController" ambiguity (legacy had a C# REST controller *and* a TS queue controller). | **This PRD = the TS queue controller + player UX.** The C# CRUD controller maps to the persisted-`Playlist` work in Phase 3.1 and is explicitly deferred (D8/N1). *If you intended the backend CRUD too, say so and I'll fold Phase 3.1 into the plan.* |

---

## 7. Success criteria

- Clicking play on any catalog medium starts it in the **draggable card**; the card can be dragged by its header, stays within the viewport, and keeps its position across navigation and across track changes.
- The **sidebar** shows the live queue, highlights the current track, and its transport (prev/play-pause/next/shuffle/repeat), remove, reorder, and "add URL" all work.
- A track **auto-advances** to the next on end; shuffle and repeat-all/-one behave correctly.
- All new code is standalone + `OnPush` + signals + `inject()`, SSR-safe, and verified end-to-end in the browser (Playwright) against a real catalog song with a YouTube medium (the Phase 2.7 verification asset: Queen → *Bohemian Rhapsody*).
