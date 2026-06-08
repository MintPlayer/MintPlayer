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

**P2 follow-up (2026-06-08): iframe drag-shield + dev-serving fix.** The embedded `<iframe>` swallows mouse events, so dragging stalled when the pointer crossed the player. Added a transparent shield over the player inside `bs-card-body`: `pointer-events: none` when idle (player controls work), toggled to `auto` on `(cdkDragStarted)`/off on `(cdkDragEnded)` (mirrors the legacy master fix). Verified live: shield present, idle `none`→active `auto`, `z-index 2`, fully covers the iframe. **Root-caused a recurring dev-server staleness:** `Program.cs` called `app.UseSpaStaticFilesImproved()` unconditionally, so the pre-built `ClientApp/dist` shadowed the live Angular CLI dev-server and froze the served bundle at the last `ng build`. Fixed by guarding it with `if (!app.Environment.IsDevelopment())` (standard ASP.NET SPA pattern) — in dev the CLI dev-server now serves live bundles; in prod the prebuilt static files serve.

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

## Phase P4 — Playlist sidebar — ✅ DONE (2026-06-08)

Goal: the docked queue panel.

**Outcome:** ✅ `src/app/player/{playlist-sidebar.ts, playlist-sidebar.scss}` created (`app-playlist-sidebar`, standalone, OnPush, browser-only). A right-docked drawer (`z-index 1045`, above the card boundary) bound to `PlayerService`: header with live count badge + close; transport bar (shuffle toggle, prev, play/pause, next, repeat-cycle with `bi-repeat`/`bi-repeat-1` + active highlight); progress row (`<bs-progress>`/`<bs-progress-bar>` + `m:ss` time labels); `<bs-list-group>` queue with now-playing highlight (`bi-volume-up-fill` + `text-primary`, matched by `entry.key`), `routerLink` deep-link (closes the drawer) or plain title, and per-row remove; an "Add URL" form gated on `MediaPlayabilityService.canPlay`. Mounted in `shell.html` after `<app-player-card/>`. **Play/pause commanding wired (deferred from P2):** the card now binds `[playerState]="player.playerState()"`, and `PlayerService` sets `playing` the instant playback starts (`playNow`/`addToQueue`-from-empty) so the binding agrees with `[autoplay]` instead of pushing an `unstarted` that fights it. **Browser-verified end-to-end** (dev admin, Queen → *Bohemian Rhapsody*): card plays → Queue button opens the drawer → transport shows **Pause** (state reflected) → clicking Pause flips to **Play** (command reaches the iframe) → progress live-updates `0:17 / 5:59` → typing a valid YouTube URL enables the gated Add button → Add grows the queue to 2 rows. **14/14 unit specs green** (added `media-resolver.spec.ts`; fixed the long-stale scaffold `app.spec.ts > should render title` → now asserts the site-wide JSON-LD).

- **P4.1** ✅ `playlist-sidebar.ts` — transport / progress / queue list / now-playing / remove / deep-link, bound to `PlayerService` signals.
- **P4.2** ⏭️ **Drag-reorder deferred — needs a framework change.** The queue engine (`@mintplayer/playlist-controller`) keeps order in a private `_playlist` with no public move/insert, and rebuilding via `setPlaylist` re-clones every entry and drops the currently-playing identity (restarts playback). A clean reorder requires a new engine method (`moveInPlaylist(from, to)`) in the playlist-controller package — tracked as a batched framework change, **not** worked around app-side. The sidebar ships without reorder; documented in the component JSDoc + PRD.
- **P4.3** ✅ "Add URL" input gated by `MediaPlayabilityService.canPlay` → `player.addToQueue(...)`.
- **P4.4** ✅ Mounted in `shell.html`; open/close bound to `player.isOpen()`; toggled from the card header Queue button. Docked `.scss` using shell/Bootstrap variables.

**Exit (browser):** ✅ sidebar lists the queue, highlights current, transport works, remove works, add-URL enqueues, auto-advance reflects in the list. (Reorder intentionally out — see P4.2.)

---

## Phase P5 — Polish, SSR, tests, docs — partially done

- **P5.1** SSR pass: confirm no `window`/drag access server-side; player card + sidebar render only in the browser; `ng build` + a prerender smoke of a song page shows no errors. _(Card + sidebar are both `isPlatformBrowser`-gated; full prerender smoke still pending.)_
- **P5.2** Accessibility/UX: focus states, keyboard on transport buttons, `aria-label`s, `cursor:move` on the handle, sensible default card position. _(All controls carry `title`/`aria-label`; toggles expose `aria-pressed`.)_ ✅ **Display-name resolved.** The Media renderers only have a URL (no track name; `Medium` has just `Value`+`TypeId`, and the column renderer has no parent song), so rather than thread a poor title the **card resolves the real title from the player** on first `playing` via `<video-player>.getTitle()` and the service overlays it on the queue display (`key → title` map; entries stay immutable). Browser-verified: card header + queue row both show *"Queen – Bohemian Rhapsody (Official Video Remastered)"*. _(Remaining: card default-position tuning is minor.)_ The sidebar's "Add URL" field is wrapped in **`<bs-form>`** so it picks up `.form-control` (shipped only via that component's `BsFormControlDirective`); the form's `(submitted)` drives the enqueue.
- **P5.3** Specs: `PlayerService` (11, P1.4 + playing-on-start + displayTitle) ✅, `media-resolver` (4) ✅, `app.spec` repaired ✅ — **17/17 green**. **Still pending:** a Playwright e2e using real `page.mouse` stepping for drag-*motion* (CDK ignores synthetic events), and a `PlaylistSidebar` component spec.
- **P5.4** Update `Implementation-Plan-Spark-Migration.md`: tick the Phase 4 *global media player* slice; add a back-reference noting Phase 3.1 will feed `PlayerService.playNow` with a saved playlist's tracks. Update these companion docs' status. _(Pending.)_
- **P5.5** _(new)_ **Framework follow-up — queue reorder.** Add `moveInPlaylist(from, to)` (or equivalent in-place reorder) to `@mintplayer/playlist-controller` so the sidebar can offer `cdkDropList` drag-reorder without restarting playback; then wire `player.reorder(...)` + `moveItemInArray`. Batch with other framework changes.

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
