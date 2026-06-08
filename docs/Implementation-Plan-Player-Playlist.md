# Implementation Plan — Draggable Media Player + Play-Queue + Playlist Sidebar

**Companion to:** [`PRD-Player-Playlist.md`](./PRD-Player-Playlist.md)
**Parent plan:** [`Implementation-Plan-Spark-Migration.md`](./Implementation-Plan-Spark-Migration.md) (this realises the *global media player* slice of **Phase 4 — Public UI**, and is a prerequisite shaped for **Phase 3.1 — Playlist**)
**Date:** 2026-06-08

This sequences the player/playlist work into small steps that each end in something runnable and browser-verifiable. It front-loads the one unknown (the queue-engine package version) into a spike so we don't build UI on a broken foundation.

---

## Guiding principles

1. **De-risk the engine first.** Prove `@mintplayer/playlist-controller@22` ↔ `ng-video-player@22` enqueue→play→ended→advance in a throwaway harness *before* building the card or sidebar (P0).
2. **Service before UI.** `PlayerService` (the store) lands and is unit-smoke-tested before any component binds to it.
3. **Mount in the Shell.** Player card + sidebar live outside the router-outlet so playback survives navigation.
4. **Match the idiom.** Standalone + `OnPush` + `signal()`/`input()`/`effect()` + `inject()`, `@if`/`@for`, hyphenated filenames without `.component`, SSR-guarded. No NgRx.
5. **Verify in the browser.** Each phase ends with a Playwright check against Queen → *Bohemian Rhapsody* (the Phase 2.7 playable YouTube medium).

---

## Phase P0 — Dependencies & engine spike — ✅ DONE (2026-06-08)

Goal: lock the foundation.

**Outcome:** ✅ `@angular/cdk@22.0.0` promoted to a direct dep (was transitive via ng-bootstrap; deduped) and `@mintplayer/playlist-controller@20.0.0` added — both resolve (`@angular/cdk/drag-drop` + the engine). The engine contract was proven by bundling it through **esbuild** (the app's real bundler — note the `@angular/build:unit-test`/vitest path *externalizes* node_modules to Node's strict ESM resolver, which rejects the package's extensionless internal imports; esbuild resolves them fine, so this is a test-harness quirk, not an app-build problem). Verified: enqueue auto-plays first; `playerEnded` advances then exhausts to `null`; `repeatAll` wraps; `repeatOne` holds on natural end while `next()` forces; shuffle yields a queued entry; removing the current entry advances. **Key finding → PRD §4.2 identity contract:** the controller clones entries and matches on object identity of its own clones, so the UI must pass back instances from `controller.playlist`. Throwaway spike files removed. _(Pre-existing unrelated failure noted: the scaffolded `app.spec.ts > should render title` expects 'Hello, ClientApp' which `App` no longer renders — not touched here.)_

- **P0.1** Add direct deps to `MintPlayer.Web/ClientApp/package.json`: `@angular/cdk@^22.0.0` (currently transitive via ng-bootstrap) and `@mintplayer/playlist-controller@^20.0.0` (**corrected from ^22**: the queue engine is the framework-agnostic core, versioned `20.x` like `@mintplayer/video-player@20` / `player-provider@20` / the `@20` plugins already present; latest published is `20.0.0`, peer `rxjs ^7.4.0` ✓). `npm install`; confirm `@angular/cdk/drag-drop` and `@mintplayer/playlist-controller` both resolve.
- **P0.2** Throwaway harness (a temporary route or spec): `new PlaylistController<{key,url,title}>()`, `addToPlaylist` two YouTube urls, subscribe `video$` → `<video-player>.setUrl`, on `playerStateChange===ended` call `playerEnded()`. Confirm: first plays, ends → second plays; `next()`/`previous()`/`shuffle`/`repeat` behave.

**Exit:** deps resolve; the engine drives the player and auto-advances. **If the package is unavailable/incompatible at v22, stop and decide:** port the one-class engine into the app (`src/app/player/playlist-controller.ts`) or raise it as a framework task. Delete the harness after.

---

## Phase P1 — `PlayerService` (the store) — ✅ DONE (2026-06-08)

Goal: the single source of truth, no UI yet.

**Outcome:** ✅ `src/app/player/{playlist-entry.ts, media-resolver.ts, player.service.ts, player.service.spec.ts}` created. `PlayerService` (root singleton) wraps `PlaylistController<PlaylistEntry>`, bridges `video$`→`currentEntry` via `toSignal`, exposes `queue/playerState/progress/shuffle/repeat/isOpen/cardPosition/hasCurrent/isPlaying` signals + command methods (`playNow/addToQueue/remove/next/previous/togglePlayPause/setShuffle/cycleRepeat/toggleSidebar/onPlayerState/onProgress`); auto-advance on `EPlayerState.ended`; honours the engine clone/identity contract (queue exposes controller instances). **8/8 spec assertions green.** Added `@mintplayer/player-progress@^20` direct dep (for the `PlayerProgress` type). **Test-harness fix (bonus):** `vitest-base.config.ts` inlines `/@mintplayer\//` so Vitest transforms (rather than externalizes) the extensionless-ESM player packages; wired via `angular.json` test `runnerConfig: true`. Without it any spec importing the player stack fails to load (the P0.2 quirk).

- **P1.1** `src/app/player/playlist-entry.ts` — the `PlaylistEntry` interface (PRD §4.1).
- **P1.2** `src/app/player/media-resolver.ts` — `resolvePlayable(media): string | null` helper (pick first `MediaPlayabilityService.canPlay`-true medium, prefer `Visible`); reused by play buttons (R2).
- **P1.3** `src/app/player/player.service.ts` — `@Injectable({providedIn:'root'})` `PlayerService` (PRD §4.2): wrap `PlaylistController<PlaylistEntry>`, bridge `video$`→`currentEntry` via `toSignal`, expose `queue/playerState/progress/shuffle/repeat/isOpen/hasCurrent` signals and the command methods. Auto-advance wired in `onPlayerState`.
- **P1.4** Smoke spec: `playNow`→`currentEntry` set; `addToQueue`→`queue` grows; `onPlayerState(ended)`→advances; `remove`/`next`/`previous`/`cycleRepeat`/`setShuffle` mutate signals as expected.

**Exit:** `PlayerService` passes its smoke spec; nothing renders yet.

---

## Phase P2 — Draggable player card — ✅ code done (2026-06-08; live browser verify folded into P3)

Goal: a floating, draggable card that actually plays.

**Outcome:** ✅ `src/app/player/player-card.ts` (`app-player-card`, standalone, OnPush): `<bs-card cdkDrag cdkDragBoundary>` with `cdkDragHandle` on the header, hosting `<video-player>` bound to `PlayerService` (`[url]` + `[autoplay]` + `(playerStateChange)`/`(progressChange)`; `[playerState]` deliberately *not* bound to avoid an initial `unstarted` fighting autoplay — play/pause command is P4). Header has queue-toggle + close buttons; `clear()` added to the service. Free-drag position persisted in `PlayerService.cardPosition` (restored via `[cdkDragFreeDragPosition]`, saved on `(cdkDragEnded)`). Browser-only (`isPlatformBrowser`), shown only while `hasCurrent()`. Mounted in `shell.html` after `</bs-shell>` (survives navigation). **`ng build` (dev) green.** Live browser verification (appears on play → plays → drag by header → stays in-viewport → persists across nav) is done in **P3**, where the real play-button trigger exists — avoids a throwaway dev button (plan item P2.4).

- **P2.1** `src/app/player/player-card.ts` (`app-player-card`, standalone, `OnPush`): `<bs-card cdkDrag cdkDragHandle …>` + `<video-player>` bound to `PlayerService` (PRD §4.3). Inline template/styles (small-component convention).
- **P2.2** Drag: `cdkDragBoundary` to a fixed full-viewport layer; persist position in a `PlayerService` signal on `(cdkDragEnded)`; restore via `[cdkDragFreeDragPosition]`. **Browser-only** render + `afterNextRender` for position.
- **P2.3** Mount `<app-player-card />` in `shell.html` (sibling of `<router-outlet>`); show only when `player.hasCurrent()`.
- **P2.4** Temporarily call `player.playNow([…])` from a dev button to verify, then remove.

**Exit (browser):** card appears on play, plays the medium, drags by its header, stays in-viewport, keeps position across navigation. Verify with Playwright.

---

## Phase P3 — Wire the existing play buttons — ✅ DONE (2026-06-08)

Goal: real entry points feed the queue.

**Outcome:** ✅ `MediaPlayButton` now injects `PlayerService` and on click calls `player.playNow([playlistEntryFromUrl(url, …)])` (added an optional `title` input); the self-contained inline overlay is gone. Both renderers (`media-column-renderer`, `media-detail-renderer`) forward `[url]` unchanged, so both now drive the global card. `ng build` (dev) green. **Browser-verified end-to-end** (P2 + P3 together, logged in as the dev admin): on Queen's detail page, clicking the Media-grid Play button makes the floating `<bs-card>` player appear bottom-right and **play the Bohemian Rhapsody YouTube medium** (screenshot captured). **Drag wiring verified** via DOM (`cdk-drag` on the card, `cdk-drag-handle` on the header, boundary layer present); CDK's drag motion is not reproducible with synthetic JS events / Playwright `dragTo`, so a faithful drag-*motion* assertion is deferred to the P5 e2e (real `page.mouse` stepping). (Bonus: confirmed `Song.Released` renders `1975-10-31` from the earlier DateOnly change.)

- **P3.1** `MediaPlayButton` (`src/app/media/media-play-button.ts`): replace the inline overlay with `inject(PlayerService)` + `player.playNow([entry])` (build the entry via `media-resolver`). Keep the `canPlay` gating.
- **P3.2** Confirm both renderers (`media-column-renderer.ts`, `media-detail-renderer.ts`) now drive the global card; remove the now-dead overlay markup/styles.

**Exit (browser):** clicking the play triangle on Queen's media grid plays *Bohemian Rhapsody* in the draggable card (not a private overlay).

---

## Phase P4 — Playlist sidebar — ~1.5 days

Goal: the docked queue panel.

- **P4.1** `src/app/player/playlist-sidebar.ts` (`app-playlist-sidebar`, standalone, `OnPush`): transport bar (shuffle/repeat/prev/play-pause/next), progress row, `<bs-list-group>` queue with now-playing highlight + `routerLink`/external-link + remove (PRD §4.4). Bind to `PlayerService` signals.
- **P4.2** Drag-reorder: `cdkDropList` + `(cdkDropListDropped)` + `moveItemInArray` → `player.reorder(...)`.
- **P4.3** "Add URL": small input gated by `MediaPlayabilityService.canPlay` → `player.addToQueue(...)`.
- **P4.4** Mount in `shell.html`; open/close bound to `player.isOpen()`; toggle in the card header (and/or shell topbar via the `bsShellTopbar` pattern). Docked styling (`.scss`), themed like the legacy sidebar but using current shell variables.

**Exit (browser):** sidebar lists the queue, highlights current, transport works, reorder persists, remove works, add-URL enqueues, auto-advance reflects in the list.

---

## Phase P5 — Polish, SSR, tests, docs — ~1 day

- **P5.1** SSR pass: confirm no `window`/drag access server-side; player card + sidebar render only in the browser; `ng build` + a prerender smoke of a song page shows no errors.
- **P5.2** Accessibility/UX: focus states, keyboard on transport buttons, `aria-label`s, `cursor:move` on the handle, sensible default card position.
- **P5.3** Specs: `PlayerService` (extend P1.4), `media-resolver`, and a Playwright e2e (play → drag → advance → reorder → remove).
- **P5.4** Update `Implementation-Plan-Spark-Migration.md`: tick the Phase 4 *global media player* slice; add a back-reference noting Phase 3.1 will feed `PlayerService.playNow` with a saved playlist's tracks. Update these companion docs' status.

**Exit:** green build, green specs, browser-verified, docs updated.

---

## File map (new / changed)

```
MintPlayer.Web/ClientApp/
  package.json                         # + @angular/cdk, @mintplayer/playlist-controller   (P0.1)
  src/app/player/                       # NEW feature folder
    playlist-entry.ts                   # P1.1
    media-resolver.ts                   # P1.2
    player.service.ts                   # P1.3  (the store)
    player.service.spec.ts              # P1.4 / P5.3
    player-card.ts                      # P2.1  (draggable <bs-card> + <video-player>)
    playlist-sidebar.ts                 # P4.1  (queue + transport + reorder)
    playlist-sidebar.scss               # P4.4
  src/app/shell/shell.html              # mount <app-player-card/> + <app-playlist-sidebar/>  (P2.3/P4.4)
  src/app/media/media-play-button.ts    # route through PlayerService; drop overlay         (P3.1)
  src/app/media/media-column-renderer.ts, media-detail-renderer.ts  # confirm/cleanup        (P3.2)
docs/
  PRD-Player-Playlist.md                # this PRD
  Implementation-Plan-Player-Playlist.md# this plan
  Implementation-Plan-Spark-Migration.md# tick Phase 4 player slice                          (P5.4)
```

---

## Sequencing

```
P0 (deps+spike) ──▶ P1 (service) ──▶ P2 (card) ──▶ P3 (wire buttons) ──▶ P4 (sidebar) ──▶ P5 (polish/SSR/tests/docs)
```

Strictly sequential — each phase depends on the prior. P3 can overlap P4 once `PlayerService` + card exist.

## Rough effort

| Phase | Estimate |
|-------|----------|
| P0 — deps + engine spike | 0.5 day |
| P1 — PlayerService | 1 day |
| P2 — draggable card | 1 day |
| P3 — wire play buttons | 0.5 day |
| P4 — sidebar | 1.5 days |
| P5 — polish/SSR/tests/docs | 1 day |
| **Total** | **~4.5–5.5 days** |

## Biggest watch-items

1. **Engine package version (R1) — settled in P0:** `@mintplayer/playlist-controller@^20` (core family), not `22`.
2. **SSR (R4)** — player card + drag are browser-only; never let drag/position logic run server-side.
3. **`@angular/cdk` promoted to a direct dep (R3)** — don't ship on a transitive resolution.
4. **Scope line (R5/D8)** — if "PlaylistController" was meant to include the **server-side persisted Playlist CRUD**, that's Phase 3.1 and expands this plan; confirm before P1.
