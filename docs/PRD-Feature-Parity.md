# PRD — Feature Parity: Legacy MintPlayer → Spark implementation

**Status:** Audit complete; **all five flagged gaps (§3.1–§3.5) implemented + verified end-to-end** (2026-06-28, no framework change required). **Date:** 2026-06-09.
**Branch:** `feature/spark-migration`.
**Companions:** [`PRD-Spark-Migration.md`](./PRD-Spark-Migration.md), [`Implementation-Plan-Spark-Migration.md`](./Implementation-Plan-Spark-Migration.md), [`PRD-Player-Playlist.md`](./PRD-Player-Playlist.md).

Legacy app: `C:\Repos\b\MintPlayer` (ASP.NET Core MVC + repo/service/DTO + EF Core + SQL Server + Elasticsearch + Angular 13 SSR/PWA).
Current app: `C:\Repos\MintPlayer` (`MintPlayer.Web` host on **MintPlayer.Spark `preview.39`** + RavenDB; `MintPlayer.Domain` entities; Angular 22 SPA `MintPlayer.Web/ClientApp`).
Framework: `C:\Repos\MintPlayer.Spark`.

> **Purpose.** Audit which important features of the legacy MintPlayer site exist in the current Spark implementation, identify gaps, and specify the approach to close the highest-value ones (the five the user flagged are detailed in §3).

---

## 1. Parity matrix (legacy → current)

Status legend: ✅ done · 🟡 partial · ❌ missing · ⏭️ deferred (later phase).

| Area | Legacy feature | Current Spark status | Gap / next |
|---|---|---|---|
| **Catalog domain** | Artist / Person / Song (TPH `Subject`), Medium + MediumType, Tag + TagCategory, join flags (Credited/Active) | ✅ all entities (`MintPlayer.Domain/Entities/*`), embedded AsDetail (Media, Members, Artists), multi-ref TagIds, sub-queries | — |
| **Admin CRUD** | bespoke MVC admin | ✅ Spark metadata-driven auto-UI (`App_Data/Model/*.json`, program-unit menus) | — |
| **Search** | Elasticsearch full-text + suggest | ✅ backend: RavenDB `Subjects_Search` multimap + `/api/search` + `/api/search/suggest` (D1 confirmed) | ❌ public search **page/autocomplete UI** (Phase 4.4) |
| **Likes / favorites** | rating/like model + like buttons on detail/lists | ✅ backend (`UserLike` + `Likes_Count` + `/api/subject/likes\|favorites`) + **`app-subject-like` widget** on subject detail (§3.3) | favorites list UI still ⏭️ |
| **Global media player** | floating draggable card, multi-provider, play-queue (shuffle/repeat/prev/next/auto-advance) | ✅ `player/` (`PlayerService` over `@mintplayer/playlist-controller`, `PlayerCard`, drag-shield) | — |
| **Playlist sidebar** | docked queue; **rows link to the song** | ✅ `PlaylistSidebar` + **rows deep-link to the subject** (§3.1: `MediaPlayButton` reads the PO route) | — |
| **Playlists (persisted)** | CRUD, public/private, ordered tracks | ✅ `Playlist` + `PlaylistTrack`: row-level security, My/Public queries, `[Sortable]` drag-reorder, **"Play this playlist" (Play/Shuffle/Add-to-queue)** via `/api/playlist/playable` (§3.2) | — |
| **Lyrics** | per-song lyrics text | ✅ `string Song.Lyrics` edited in the **song form** via the new Spark **`MultiLineString`** datatype (PR #204) + **RavenDB revisions on Songs** (§3.4) | — |
| **Karaoke sync** | per-line timeline (`int[]×20`, ÷20), sync editor + player highlight | ✅ `Song.LyricsTimings` (per-medium `StartTimes[]`) + Editor "Sync timing" tool + progress-driven highlight (§3.5) | — |
| **Catalog public pages** | artist/person/song detail (custom layout, embedded player, like widget, share, SEO) | ❌ only admin auto-UI + placeholder home | ⏭️ Phase 4.3 (bespoke public site) |
| **Home / public playlists / blog reading / GDPR** | public pages | ❌ | ⏭️ Phase 4.5 |
| **Blog** | `BlogPost` + reading/admin | ❌ no entity/pages | ⏭️ Phase 3.3 |
| **Auth — baseline** | register/login/confirm/reset, 2fa-verify | ✅ Spark auth (Identity/RavenDB, cookie+XSRF), `sparkAuthRoutes()`, email sender, dev seeder | — |
| **Auth — 2FA enrollment** | QR enable/disable, recovery codes | ❌ | ⏭️ Phase 5.2 |
| **Auth — social login** | Google/Microsoft/Facebook/GitHub | ❌ no backend wiring or buttons | ⏭️ Phase 5.3 |
| **Auth — passkeys/WebAuthn** | Fido2NetLib, register/list/passwordless | ❌ (D3: full parity in v1) | ⏭️ Phase 5.5 |
| **Auth — account linking / profile / change-pw / email-confirm pages** | yes | ❌ | ⏭️ Phase 5.1/5.4 |
| **Public REST API** | `api/v1/*` signed JWT (other instances consume) | ❌ (D5: keep) | ⏭️ Phase 6.6 |
| **SEO endpoints** | sitemap, robots, AMP song page, OpenSearch, per-entity JSON-LD, OpenGraph, hreflang | 🟡 site-wide JSON-LD (`WebSite`/`Organization`) + canonical + title strategy + meta desc | ❌ per-entity JSON-LD, sitemap/robots/AMP/OpenSearch (Phase 6.3) |
| **SSR / prerendering** | Angular SSR/PWA | ❌ referenced (`MintPlayer.AspNetCore.SpaServices`) but **not wired** (no `main.server`/`OnSupplyData`/`UseSpaPrerendering`) | ⏭️ Phase 4.3 (D2) |
| **PWA** | service worker + manifest | ✅ `provideServiceWorker` + `ngsw-config.json` + manifest/icons | — |
| **i18n** | languages | ✅ Spark `SparkLanguageService` (en/fr/nl) for auto-UI | 🟡 no public-content i18n yet (little public content) |
| **Fetcher / scrapers** | **9 LYRICS scrapers** (AZLyrics, Genius, LoloLyrics, Lyrics.com, Musixmatch, Muzikum, SongLyrics, SongMeanings, SongtekstenNet) + `IFetcherService` + `POST /web/v3/fetcher` | ❌ not ported | ⏭️ Phase 6.2 (feeds the lyrics feature, §3.4) |
| **Roles** | User / Moderator / Blogger / Administrator | 🟡 Everyone / Administrator / Blogger / **Editor** (security.json; Editor = trusted catalog/lyrics curator) | ❌ no **Moderator** group (Editor now covers catalog curation) |
| **Background jobs / messaging / durable email** | DB job queue, scheduled tasks | ❌ | ⏭️ Phase 6.4/6.5 |
| **Data migration tool** | n/a | 🟡 only Phase-0 spike (now removed); full `MintPlayer.Migration` is Phase 7 | ⏭️ Phase 7 |
| **LogEntry / request logging** | yes | ❌ | ⏭️ optional (Phase 3.4) |

> The legacy column is filled from the migration plan's per-entity mapping + phase breakdown; the **legacy investigation agent** will enrich exact paths/behaviors (esp. lyrics timeline shape + the like/rating UI component) — see §4.

---

## 2. Current implementation — authoritative inventory (verified)

> From the completed current-app inventory. Everything below is **implemented** unless noted.

### Entities (`MintPlayer.Domain/Entities/`)
`Entity` (Id, audit `CreatedAt`/`ModifiedAt`, soft-delete `IsDeleted`/`DeletedAt`, `OldId`); `Subject` (`List<Medium> Media`, `[Reference] List<string> TagIds`); `Person`/`Artist`/`Song : Subject`; `Medium` (Value, `[Reference] TypeId`); `MediumType`; `Tag` (Description, `[Reference]` Category + self-ref Parent); `TagCategory` (Description, `Color`); `SongArtist`/`ArtistMember` (ref + flag); `UserLike` (`UserLikes/{userId}`, `Likes`/`Dislikes` id arrays); `Playlist` (Name, Description, IsPublic, OwnerId, `[Sortable] List<PlaylistTrack> Tracks`); `PlaylistTrack` (`[Reference] SongId`, order = array position); `MintPlayerUser : SparkUser` (PictureUrl, Bypass2faForExternalLogin).

### Backend (`MintPlayer.Web/`)
- **Controllers:** `SearchController` (`/api/search`, `/api/search/suggest`); `SubjectController` (`/api/subject/likes` GET+POST, `/api/subject/favorites`). `/api` excluded from SPA fallback.
- **Indexes/projections:** `Subjects_Search` (multimap + Suggestion), `Likes_Count` (fan-out map-reduce), `People_Overview`→`VPerson` (FullName); `VSubject`, `LikeCount`, `SubjectLikeModels`.
- **Actions:** `EntityActions<T>` base (audit + soft-delete); `PersonActions`/`ArtistActions`/`SongActions`/`TagActions` (sub-queries); **`PlaylistActions`** (row-level `IsAllowedAsync` admin/owner/public; owner stamp; `My_Playlists`/`Public_Playlists`).
- **Host:** `Program.cs` (`AddSpark`+context, auth, email, controllers+spark, dev `UseAngularCliServer`); `MintPlayerSparkContext` (7 `!IsDeleted` queryables); `DevDataSeeder`; `MintPlayerEmailSender`; `App_Data/security.json` (Everyone/Administrator/Blogger; Playlist write granted to Everyone, gated by row-level `IsAllowedAsync`); `App_Data/programUnits.json` (Music / Playlists / Catalog menus).

### Angular SPA (`MintPlayer.Web/ClientApp/src/app/`)
- **`player/`:** `PlayerService` (signals store over `@mintplayer/playlist-controller`; playNow/addToQueue/remove/next/prev/togglePlayPause/shuffle/repeat; auto-advance; player-resolved title overlay), `PlayerCard` (draggable `<bs-card cdkDrag>` + `<video-player>` + iframe drag-shield), `PlaylistSidebar` (transport/progress/queue/remove/add-URL + topbar toggler), `PlaylistEntry` (`{key,url,title,routerLink?}`) + `media-resolver`.
- **`media/`:** `MediaPlayButton` (→ `player.playNow`), `MediaPlayabilityService` (`findApis`), `video-player-plugins` (youtube/vimeo/dailymotion/soundcloud/spotify/file).
- **`renderers/`:** color-swatch (`color-swatch`); `media-column`/`media-detail` (`media-player` on `Medium.Value`).
- **shell/config:** `<bs-shell>` (brand, language picker, `<spark-auth-bar>`, playlist toggler) + program-unit sidebar + outlet; mounts player card + sidebar outside the outlet. `app.config.ts` (provideSparkAuth/ClientOperations/AttributeRenderers/VideoApis, PWA, title strategy, base href); `app.routes.ts` (`sparkAuthRoutes` + home + `sparkRoutes`); site-wide JSON-LD in `app.ts`.

---

## 3. The five flagged gaps — designs

> **Current state** confirmed from the current inventory; **Recommended approach** enriched by the gap deep-dive agent (file pointers + data shapes below).

### 3.1 Playlist-sidebar: click a queue entry to open the song
- **Current state:** `PlaylistEntry.routerLink?` exists and `playlist-sidebar.ts` renders `<a [routerLink]>` when present (~L98-103), cleanly separating ad-hoc URL entries (no link) from catalog songs (with link). Nothing populates it: `MediaPlayButton`/`playlistEntryFromUrl(url,{title})` never set `routerLink`; the media renderers (`media-column`/`media-detail`) only get the `Medium.Value` URL, **not the parent Song id**.
- **Recommended approach (app-side):** surface the parent subject id in the renderers (Spark passes `value`/`attribute`/`options`/`formData` to a detail renderer — the parent entity id is reachable from that context). Add `parentId`/`parentType` inputs to `MediaPlayButton`, forward into `playlistEntryFromUrl(url, { title, key: songId, routerLink: ['/po','song', songId] })`. Use the **song id as `key`** (dedupe across media). Sidebar rows then link to `/po/song/{id}`. (NB: SPA route is `/po/{type}/{id}` — no `/spark` prefix.)
- **Legacy parity:** the old sidebar navigated on row click (`router.navigate(['/song', songId, slug])` / a `songClicked` emitter), so clickable rows are a parity target, not a new idea.
- **Scope:** **app-side.**

### 3.2 "Play this playlist"
- **Current state:** absent. `PlayerService.playNow(entries)` is shaped for it; nothing feeds a persisted `Playlist`'s tracks in.
- **Finding:** Spark **does** support custom actions (`ICustomAction`, registered in `customActions.json`, executed via `POST /spark/actions/{type}/{actionName}`) — **but they run server-side and return void** (`spark.service.ts` ~L124-130); a custom action **cannot return `PlaylistEntry[]` to the client** today.
- **Recommended approach (app-side):** a **custom Angular Playlist detail button** that calls a new app endpoint **`GET /api/playlist/{id}/playable` → `PlaylistEntry[]`** (server resolves each track's Song → first playable Medium, avoiding client N+1), then calls `player.playNow(entries)`. Optional later framework enhancement: let custom actions return client operations/data (Spark PR) — not required now.
- **Legacy parity (offer all 3 modes):** the old "play playlist" had **Play Now** (clear queue, load all, shuffle off), **Shuffle** (load all, shuffle on), and **Add to Queue** (append) — `ePlaylistPlaybutton`. `PlayerService` already has `playNow`/`addToQueue`/`setShuffle`, so the button can expose all three.
- **Scope:** **app-side** (+ optional framework enhancement).

### 3.3 Like-button UI
- **Current state:** backend complete (`UserLike` + `/api/subject/likes|favorites`); **no UI**.
- **Legacy reference:** `subject-like.component.ts/.html` (`C:\Repos\b\MintPlayer\...\components\subject\subject-like`) — two buttons (thumbs up/down) + loading state, `@Input() subject`, calling `subjectService.like()`. Legacy data: `Like {SubjectId, UserId, DoesLike:bool}`; `SubjectLikeResult {Likes:int, Dislikes:int, Like:bool?, Authenticated:bool}` — the current `/api/subject/likes` GET returns the same shape. Legacy also had a **favorites** list (`/api/v1/subject/favorite`, `/song/favorite`) → current `/api/subject/favorites` covers it (UI still needed). Shown on Song/Artist/Person detail.
- **Recommended approach (app-side):** a standalone `app-subject-like` widget: `GET /api/subject/likes?id={subjectId}` (anon read → counts + caller state); `POST /api/subject/likes {subjectId, like:true|false|null}` on click. Must send the **`X-XSRF-TOKEN`** header (cookie-auth POST; Spark's Angular identity interceptor should already attach it — verify). Surface on subject detail (custom detail renderer or component); CSRF caveat is the same deferred one (hardened surface = future JWT API).
- **Scope:** **app-side.**

### 3.4 Song lyrics (+ RavenDB revisions) — ✅ DONE (final design)
- **Model:** lyrics **text** is a plain `string? Song.Lyrics` (newline-delimited), **edited in the standard Spark song form** like the rest of the catalog — not a bespoke editor. Karaoke timing lives separately on `Song.LyricsTimings: List<LyricsTiming>` (hidden from the form; see §3.5).
- **MultiLineString datatype (framework):** the form needed a `<textarea>`, which Spark lacked. Added a **`MultiLineString` datatype** to MintPlayer.Spark (PR #204, shipped `preview.41` / `ng-spark 22.0.8`): the model synchronizer **preserves** a hand-set `"dataType": "MultiLineString"` on a string attribute across re-sync (no `[MultiLine]` C# attribute — it's a model-JSON presentation override); `spark-po-form` renders a textarea (top-level + inline cells), `spark-po-detail` renders pre-wrapped text. `Song.json` → `Lyrics` is `MultiLineString`, visible.
- **Revisions — app-side, no framework seam needed:** the anticipated `IDocumentStore` hook was unnecessary — `AddSpark` already registers `IDocumentStore` in DI, so `RevisionsConfigurator.cs` (called from `Program.cs`) sends `ConfigureRevisionsOperation` for the `Songs` collection at startup. Verified: 3 revisions recorded for an edited song.
- **Edit rights:** lyrics text is catalog data → editable by the new **Editor** group + Administrators (see §6). `SongLyricsController` GET (`text`+`timings`+`canEdit`, anon) serves the karaoke read-model.
- **Scope:** **framework** (MultiLineString, done + published) + **app-side** (everything else).

### 3.5 Karaoke timestamp synchronization — ✅ DONE (final design)
- **Model:** `LyricsTiming { MediumUrl, List<double?> StartTimes }` on `Song.LyricsTimings`. `StartTimes` runs **parallel to the lyrics text lines** (legacy's flat-array idea) but is **keyed per medium URL** (the user's call — YouTube vs Spotify timing differs), a deliberate improvement over legacy's per-(song,user) array. Hidden from the song form (edited via the sync tool below).
- **Editor + highlight:** `app-song-lyrics` (in `lyrics/`, mounted by `AppPoDetail`): read-only lyrics display with the active line highlighted from `PlayerService.progress()` (latest line whose start ≤ currentTime) for the currently-playing medium; an Editor-only **"Sync timing"** mode with a per-line "Set" button that stamps `progress().currentTime` into `StartTimes[i]` while the song plays. Saves via **`PUT /api/song/lyrics/timings`** (Editor/Administrator-gated; touches only `LyricsTimings`).
- **Scope:** **app-side.**

---

## 4. Team findings — folded in ✅

All three investigations (current-app inventory, legacy inventory, 5-gap deep-dive) are complete and folded into §1–§3 above; key legacy specifics are preserved in Appendix A. Resolved questions:
- §3.1 — renderer reads parent subject id from the Spark detail-renderer context (`formData`); thread it through `MediaPlayButton`. **App-side.**
- §3.2 — Spark custom actions exist but **return void** (can't return entries to client) → use a custom detail button + `GET /api/playlist/{id}/playable`. **App-side.**
- §3.3 — legacy `subject-like.component` (thumbs up/down); current API already matches the legacy result shape. **App-side widget.**
- §3.4 — **no `IDocumentStore` config seam in Spark** → revisions need a **framework change** (`ISparkBuilder.ConfigureDocumentStore`), with a fragile app-side `IDocumentStore`+`ConfigureRevisionsOperation` fallback. → candidate Spark issue (§5).
- §3.5 — legacy timeline is a flat `List<double>` of seconds per-(song,user); modernize to per-line `StartTime` keyed to a medium. **App-side.**

---

## 5. Framework dependencies (MintPlayer.Spark issues filed during migration)

| # | Topic | Status |
|---|---|---|
| #185 / #186 | AsDetail reference **breadcrumbs** resolve by id (not first options page) | ✅ shipped (preview.38) + consumed |
| #187 / #188 | AsDetail-array **drag-to-reorder** (`[Sortable]`) + inline-edit parity | ✅ shipped (preview.39 / ng-spark 22.0.6) + consumed (Playlist.Tracks) |
| #189 | Inline AsDetail Reference cell → **ellipsis+modal query picker** (`referenceDisplayType`) | 🟢 open (Playlist.Tracks SongId currently a `<select>`) |
| #204 | **`MultiLineString` datatype** — string attribute renders as a `<textarea>` (form, top-level + inline) + pre-wrap (detail); synchronizer preserves it as a JSON-only presentation override | ✅ shipped (`preview.41` / `ng-spark 22.0.8`) + consumed (Song.Lyrics) |
| (potential) | RavenDB **revisions** config seam (for §3.4 lyrics history) | ✅ **not needed** — `AddSpark` registers `IDocumentStore` in DI, so the app configures revisions itself (`ConfigureRevisionsOperation` in `RevisionsConfigurator.cs`, called from `Program.cs`). No framework change. |

---

## 6. Status — the five flagged gaps (all ✅ done + verified 2026-06-28)

1. ✅ **§3.1 sidebar song links** — `MediaPlayButton` reads the PO detail route (`/po/:type/:id`) and sets `PlaylistEntry.routerLink`/`key`; sidebar + playlist-sourced rows deep-link to the subject. Verified: playing `Songs/6`'s medium → queue row links `/po/song/Songs/6`.
2. ✅ **§3.2 "Play this playlist"** — `GET /api/playlist/playable?id=` (one batched load, row-level read auth) → `PlaylistPlaybackService` picks the first playable medium per track → `PlayPlaylistButton` (Play / Shuffle / Add-to-queue) in the detail toolbar. Verified on "Bohemian Favourites".
3. ✅ **§3.3 like-button UI** — `app-subject-like` (thumbs up/down + live counts) on Person/Artist/Song detail; anon read, signed-in toggle. Verified: like persisted, count → 1.
4. ✅ **§3.4 lyrics** — lyrics **text** is `string? Song.Lyrics`, edited in the **standard song form** as a new Spark **`MultiLineString`** datatype (PR #204, `preview.41`/`22.0.8`); **RavenDB revisions on Songs enabled app-side** (`RevisionsConfigurator`, no framework seam needed — `IDocumentStore` is in DI). Verified: Editor edited via the PO-Edit textarea, persisted, 3 revisions, timing preserved.
5. ✅ **§3.5 karaoke sync** — `Song.LyricsTimings: [{MediumUrl, StartTimes[]}]` (per-medium parallel array, hidden from the form); `app-song-lyrics` = read-only display + progress-driven highlight + Editor-only "Sync timing" (per-line "Set") saving via `PUT /api/song/lyrics/timings`. Verified end-to-end as the Editor.

**Edit rights:** lyrics/catalog editing moved off Administrator-only to a new trusted **Editor** group (`security.json`: `QueryReadEditNewDelete` for Person/Artist/Song + embedded Medium/SongArtist/ArtistMember; dev `editor@mintplayer.com` seeded). Verified: the Editor edits Songs + syncs timing without Administrator.

**Infra used (reusable):** `AppPoDetail` wrapper (`sparkRoutes({poDetail})`) + `PoContextService`/`PoContextCapture` host all per-type detail-page additions (play button, like widget, lyrics) with zero extra metadata fetches.

Remaining deferred phases (unchanged): public detail pages (4.3), search page (4.4), auth long-tail (5), SEO endpoints (6.3), API (6.6), crawler (6.2), migration tool (7).

---

## Appendix A — Legacy reference (key shapes, endpoints, paths)

Compact capture of the legacy investigation (root: `C:\Repos\b\MintPlayer`; backend `MintPlayer.Data/Entities`, `MintPlayer.Web/Server/Controllers/{Api/V1,Web/V3}`, DTOs `MintPlayer.Dtos`; Angular `MintPlayer.Web/ClientApp/src/app`). Stack: ASP.NET Core MVC + EF Core/SQL Server + Elasticsearch + Angular 13 SSR/PWA.

**Data shapes**
- `Lyrics` (per-song **and per-user**): `SongId:int`, `UserId:Guid`, `Text:string` (newline-delimited), `Timeline:List<double>` (seconds, 1:1 with lines, null=unsynced), `UpdatedAt`.
- `Like`: `SubjectId:int`, `UserId:Guid`, `DoesLike:bool`. Read DTO `SubjectLikeResult {Likes:int, Dislikes:int, Like:bool?, Authenticated:bool}`.
- `Playlist` (`User`, `Description`, `Accessibility: Private|Public`, soft-delete) + junction `PlaylistSong {PlaylistId, SongId, Index}` (ordering via `Index`).
- `Medium {Type: eMediumType, Value: url}`; providers YouTube/DailyMotion/Vimeo/SoundCloud/Spotify/Apple.
- `BlogPost {Title, Headline, Body(html), audit, soft-delete}` (Blogger/Administrator).
- `WebAuthnCredential {UserId, CredentialId:byte[], PublicKey:byte[], UserHandle:byte[]?, SignatureCounter:uint, CredType, AaGuid, DisplayName, RegDate, LastUsed}` (Fido2NetLib).
- Jobs: `Job {Status: EJobStatus}`, `ElasticSearchIndexJob {Subject, SubjectStatus: eSubjectAction}`; `LogEntry` (request/error logging).

**Notable endpoints**
- Lyrics/karaoke: `GET /api/v1/song/{id}/lyrics`; `PUT /api/v1/song/{id}/timeline` (editor `/song/:id/sync`); player highlight = `interval(50)` poll, last line with `Timeline[i] < currentTime`.
- Likes/favorites: `GET|POST /api/v1/subject/{id}/likes`; `GET /api/v1/subject/favorite`, `GET|POST /api/v1/song/favorite`.
- Playlists: `GET/POST/PUT/DELETE /api/v1/playlist[...]` (+ `/my`, `/public`, paged); Web v3 mirror with CSRF.
- Auth: local + JWT (`/api/v1/account/login`); 2FA (`/web/v3/account/two-factor-*` — setup/login/recovery/generate-codes/bypass); WebAuthn (`/web/v3/account/webauthn/{register,login,credentials}/*`); social (`/web/v3/account/{providers,connect,add,logins}/*` with linking + `OtherAccountException`); register/verify/password-reset/change.
- SEO: `GET /sitemap.xml` + `/sitemap/{subject}/{count}/{page}` (video+image+hreflang fr/nl), `GET /robots.txt`, AMP `GET /amp/song/{id}`; JSON-LD `MusicRecording`+`VideoObject`+`MusicGroup`; OpenGraph + Twitter player/summary cards.
- Search: `POST /api/v1/subject/search` + `/search/suggest` (`SearchRequest {SubjectTypes[], SearchTerm}` → `SearchResults {Songs,Artists,Persons}`).
- Fetcher: `POST /web/v3/fetcher` (`{Url}` → matched subject/media) over the 9 lyrics scrapers via `IFetcherService`.

**Player/UI**
- Global floating/draggable `<video-player>` wired in `app.component.ts` (player `ViewChild`, `playerState$`, `PlayerProgress`); queue via `@mintplayer/ng-playlist-controller` (shuffle; repeat `noRepeat|repeatOne|repeatAll`; prev/next; auto-advance on `playerEnded`; add-URL with validation).
- Playlist sidebar: queue list, now-playing highlight, remove, **row click → song page**.
- i18n: `@ngx-translate` with `assets/i18n/{en,fr,nl}.json` (`?lang=` switch). PWA + Angular Universal SSR (data injected via DI, `SERVER_SIDE` token).

**Not found in legacy:** comments, in-app notifications, payments/premium, rate limiting, explicit GDPR data-export endpoint.
