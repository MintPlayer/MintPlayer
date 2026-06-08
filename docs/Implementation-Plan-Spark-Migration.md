# Implementation Plan — MintPlayer on MintPlayer.Spark

**Companion to:** [`PRD-Spark-Migration.md`](./PRD-Spark-Migration.md)
**Date:** 2026-06-06

This plan sequences the rewrite into phases that each end in something runnable. It front-loads the two highest-risk unknowns (SEO/SSR and search) into a spike so we don't discover a wall halfway through.

---

## Guiding principles

1. **De-risk first.** Phase 0 proves the load-bearing assumptions (SEO strategy, RavenDB search, auth wiring, data migration shape) before committing to the full build.
2. **Vertical slices.** Each entity is taken end-to-end (model → index → security → admin UI → public page) rather than doing all models, then all UIs.
3. **Single full-swoop cutover.** The current SQL/EF MintPlayer keeps serving production *only while the new app is built and validated in staging*. At cutover the new app fully **replaces** it in one step — the two never run side-by-side in production and there is **no SQL↔RavenDB synchronization** (D7). Validate exhaustively in staging first; rollback is a manual redeploy of the old build against a retained SQL backup, not a live fallback.
4. **Delete as you go.** A boilerplate layer (repo/service/mapper/DTO) is removed only once its replacement is proven, keeping `master` green.
5. **Contribute gaps upstream.** Net-new auth UI (2FA enrollment, social buttons, passkeys) is built so it can land in `ng-spark-auth`, benefiting both repos.

---

## Phase 0 — Spikes & decisions (de-risk) — ~1–2 weeks

Goal: turn the PRD's open decisions (D1–D7) into committed answers with evidence.

| # | Spike | Output |
|---|-------|--------|
| 0.1 | **SEO/prerendering spike (D2 — resolved).** Wire `MintPlayer.AspNetCore.SpaServices.Prerendering` into a Spark Angular app (per the `Demo/Prerendering` reference); render one public song page through `@angular/platform-server` and inject data via `OnSupplyData`. Confirm `SparkService` works under server-side HTTP and that lazy admin chunks are excluded from prerender. Test what crawlers/social scrapers receive. | Proven prerendering wiring; SSR-safe `SparkService` pattern. |
| 0.2 | **Search spike (D1).** Load a prod-scale sample into RavenDB; build `Artists/People/Songs` full-text indexes; benchmark search + `SuggestUsing` autocomplete latency and quality vs. current Elasticsearch. | Go/no-go on dropping Elasticsearch. |
| 0.3 | **Auth wiring spike.** Stand up `AddSparkFull` + `security.json` + `sparkAuthRoutes()`; verify login/register/reset/2fa-verify; chain `AddGoogle`. | Confirmed auth baseline; list of net-new auth UI. |
| 0.4 ✅ | **Data-migration spike (R5). — DONE, see `spikes/Spike.Migration`.** Migrate one aggregate (Artist + its members + media + tags + likes) from a SQL snapshot; verify references resolve via `OldId`. **Plus an auth round-trip:** migrate one user's `PasswordHash` + `SecurityStamp` and verify the existing password logs in to the Spark app (proves D8 hash portability); and migrate `AuthenticatorKey` + `TwoFactorEnabled` + recovery codes, verifying a live authenticator code validates (proves TOTP continuity). | **Proven (5/5 tests green).** Verbatim Identity-V3 hash authenticates via Spark's real `UserManager`+`UserStore`; legacy lower-iteration hash still verifies and is flagged `SuccessRehashNeeded` (forward-compatible). A code from an independent RFC-6238 generator validates through Identity's `AuthenticatorTokenProvider` against the verbatim-copied key. TPH→3 collections, embedded `Active`/`Credited` flags, cross-collection `[Reference]` IDs, Tag self-tree, and ARGB colour all round-trip; deterministic IDs make re-runs idempotent. |
| 0.6 | **Public-API spike (D5).** Stand up one `ApiController` injecting `IAsyncDocumentSession` + a JWT bearer scheme alongside Spark auth; confirm a token-authenticated `GET /api/v1/artist/{id}` queries RavenDB and coexists with the Spark middleware. | Confirmed public-API pattern (already validated against `WebhooksDemo`). |
| 0.5 | **Repo/solution shape (D6).** Decide structure (e.g. `MintPlayer.Web` Spark host + `ClientApp` + `MintPlayer.Migration` ETL tool). | Skeleton solution committed. |

**Exit criteria:** D1–D7 answered; risks R1/R4/R5 quantified; skeleton solution builds and serves the Fleet-style demo.

---

## Phase 1 — Foundation — ~2 weeks

Goal: a running Spark host with RavenDB, auth, and one entity fully working through admin + public.

- **1.1 ✅** Scaffold the ASP.NET Core 10 Spark host. **DONE** — `MintPlayer.Web` host using `AddSpark`+`UseContext<MintPlayerSparkContext>` (DemoApp pattern, not AllFeatures — avoids unused messaging/replication), RavenDB via `Spark:RavenDb` (db `MintPlayer`), `MintPlayer.Domain` entity lib, `MintPlayer.slnx`. Spark consumed via **published NuGet packages** (`10.0.0-preview.35` set, after the Abstractions publish-skew was fixed — see packaging note). **Docker Compose done** (`docker-compose.yml` + `MintPlayer.Web/Dockerfile`: single-context build now that Spark is on NuGet; ravendb 7.2 + web; `docker compose config` validates — full image build pending a running daemon).
- **1.2 ✅** `MintPlayerUser : SparkUser` (`PictureUrl`, `Bypass2faForExternalLogin`); `security.json` groups (`Everyone`/`Administrator`/`Blogger`); `spark.AddAuthorization` + `spark.AddAuthentication<MintPlayerUser>`; cookie `.SparkAuth.MintPlayer` + XSRF. **DONE** (verified `/spark/auth/*` mapped, anonymous Everyone read). **`IEmailSender` done** — `MintPlayerEmailSender : IEmailSender<MintPlayerUser>` (MailKit, `Smtp` config section, branded HTML for confirm/reset; logs instead of sending when no host configured). Verified: register → confirmation email logged with a working `/spark/auth/confirmEmail` link.
- **1.3 ✅** Angular **22** app at `MintPlayer.Web/ClientApp` (nested for SSR): `sparkAuthRoutes()`+`sparkRoutes()`, `bs-shell` shell (bsShellTopbar/bsShellSidebar, program-unit sidebar, auth bar, lang picker), `provideSparkAuth`/`provideSparkClientOperations`/`provideSparkAttributeRenderers`. **DONE** (SPA + Spark API serve together). Single shell for now — public/admin split deferred. **NB:** required a framework fix — ng-spark/ng-spark-auth realigned to ng-bootstrap 22 (datatable merge + toggle→checkbox), shipped as **`@mintplayer/ng-spark@22.0.0`** (Spark PR #179), now consumed here.
- **1.4 ✅** Shared entity base + conventions. **DONE** — `MintPlayer.Domain/Entities/Entity.cs` (abstract base: `Id`, audit `CreatedAt`/`ModifiedAt`, soft-delete `IsDeleted`/`DeletedAt`, migration `OldId`); `MediumType : Entity`. `EntityActions<T> : DefaultPersistentObjectActions<T>` centralises the conventions (stamps CreatedAt on create / ModifiedAt on edit; `OnDeleteAsync` soft-deletes; `OnLoadAsync`/`OnQueryAsync` hide deleted), abstract so the actions generator skips it — `MediumTypeActions` is the concrete adopter. `MintPlayerSparkContext.MediumTypes` filters `!IsDeleted` (Raven auto-index predicate) so the named-query/datatable path excludes deleted rows too. Audit fields hidden+read-only in `MediumType.json` (idempotent across re-sync).
- **1.5 ✅** **Vertical slice: `MediumType`** — model, query, security rights, model-sync, CRUD all proven, **and verified end-to-end through the admin auto-UI in the browser** (login → create → edit → delete against RavenDB). `App_Data/programUnits.json` adds the Catalog → Medium types sidebar menu (query alias `medium-types`). Closed the 1.2 auth gap: `DevDataSeeder` (dev-only, idempotent) seeds an Administrator; group membership is a `"group"` claim valued with the group **name** (`AccessControlService` resolves name→id via security.json translations — a GUID-valued claim matches nothing). Browser run confirmed audit stamping + soft-delete live (deleted row drops out of the datatable + 404s on load; doc retained with `IsDeleted`/`DeletedAt`).
- **1.6 ✅** PWA + SEO base. **DONE** — PWA via `ng add @angular/pwa` (`@angular/service-worker` + `ngsw-config.json`, branded `manifest.webmanifest` + icons, `provideServiceWorker` gated on `!isDevMode()` so it's inert in dev and served statically by the host in prod; prod build emits `ngsw.json` + workers; initial-bundle budget raised to 1MB/2MB). SEO base uses **`@mintplayer/ng-seo@22.0.0`** (+ peers `ng-base-url`, `ng-router-provider`): site-wide JSON-LD (`WebSite` + `Organization`) via the `[jsonLd]` directive in `app.html`, per-page canonical via `[canonicalUrl]` (absolute URLs from `provideBaseHref()`), plus `MintPlayerTitleStrategy` ("<page> | MintPlayer", kept because the package's `[seo]` title is per-page + all-or-nothing — less suited to the Spark auto-UI pages) and a default meta description in `index.html`. Verified in the browser (title, canonical `https://…/home`, description, manifest, both JSON-LD blocks as `application/ld+json`). A JSON-LD MIME bug in `ng-seo@22.0.0` (`application/json`, which crawlers ignore) was fixed upstream and shipped as **`22.0.1`** — consumed here and re-verified crawler-correct (0 stale `application/json` scripts).

**Exit criteria:** Log in, manage MediumTypes in the admin UI, deploy via Docker; i18n + PWA + SEO scaffolding in place. **✅ MET** _(Status 2026-06-07: 1.1–1.6 done; auth + MediumType admin CRUD verified end-to-end against RavenDB; i18n + PWA + SEO base in; IEmailSender done; Docker Compose authored + config-validated, image build pending a running daemon. **Phase 1 complete.**)_

> **Packaging note (2026-06-07):** the framework moved from cross-repo `ProjectReference` to **`PackageReference` @ `10.0.0-preview.35`**. An earlier swap to `preview.34` failed (NU1102 — `MintPlayer.Spark` `.34` required `MintPlayer.Spark.Abstractions >= .34`, but Abstractions had stalled at `.20`); a packaging CI run then published a coherent `.35` set (incl. Abstractions), unblocking the switch. Packages are on nuget.org (auth-free) and the MintPlayer GitHub NuGet feed (immediate, auth-required). The NuGet move is what makes the Docker image a clean single-context build. **Since bumped to `10.0.0-preview.36`** (Phase 2.2 — native multi-reference round-trip); npm `@mintplayer/ng-spark@22.0.2` + `@mintplayer/web-components@2.0.1` consumed alongside.

---

## Phase 2 — Core catalog domain — ~3–4 weeks

Goal: model and CRUD-enable the full catalog; admin screens done via auto-UI; custom renderers built.

Vertical slices, in dependency order:

- **2.1 ✅ Tag + TagCategory** — `TagCategory`(Description, `System.Drawing.Color`) + `Tag`(Description, `[Reference]` Category, self-`[Reference]` Parent). **color-swatch renderer** (column/detail/edit, ng-bootstrap picker) built + registered. Hierarchy via parent-scoped child sub-queries (`Category_Tags`, `Tag_Children`) shown on detail pages. (Standalone tag-tree visualization deferred — sub-queries cover hierarchy management.) Verified in browser incl. create.
- **2.2 ✅ Person** — `Subject` base (Media `AsDetail` of `Medium`{`[Reference]`MediumType,Value}; `TagIds`) on `Entity`; `Person`(FirstName, LastName, Born, Died). **`VPerson` query-type + `People_Overview` index** computes `FullName` (display + full-text, filters `!IsDeleted`). Tags are a **native multi-reference**: `[Reference(typeof(Tag),"GetTags")] List<string> TagIds` → searchable multi-select picker + chip display, clean `string[]` round-trip. Verified end-to-end (David Bowie + Rock). Required framework work (shipped + consumed): `MintPlayer.Spark` `preview.36` (primitive/reference array round-trip), `@mintplayer/ng-spark` `22.0.2` (multi-reference editor), `@mintplayer/web-components` `2.0.1` (tree-select pre-render null-guard).
- **2.3 ✅ Artist** — `Artist`(Name, YearStarted, YearQuit, `Members` AsDetail `{PersonRef, Active}`). `Person_Artists` sub-query (bands a person is in) on the Person detail. Verified (Raven `.Any()` over embedded array).
- **2.4 ✅ Song** — `Song`(Title, Released, `Artists` AsDetail `{ArtistRef, Credited}`). `Artist_Songs` sub-query (an artist's songs) on the Artist detail. Verified (Queen ↔ Bohemian Rhapsody ↔ Freddie round-trip).
- **2.5 ✅ Cross-type search index** — `Subjects_Search : AbstractMultiMapIndexCreationTask<VSubject>` maps `Artist`/`Person`/`Song` into one `VSubject` shape (`SubjectType`, full-text `Text`, `TagIds`; soft-deleted excluded at index time), with `FieldIndexing.Search` + `Suggestion` on `Text`. Exposed via a custom anonymous **`SearchController`** (`GET /api/search` full-text + prefix + optional `type` filter; `GET /api/search/suggest` typo-tolerant autocomplete via RavenDB `SuggestUsing`) — *not* a Spark query-type, since it fans across three collections and the framework's datatable search is in-memory post-query and doesn't pass the term to custom queries. `/api` excluded from the SPA fallback. **This is the D1 go/no-go evidence: RavenDB-native search + suggest replaces Elasticsearch.** Verified end-to-end (Bowie/Queen/Bohemian Rhapsody hits with populated projection via `.ProjectInto<>`; `type` filter ANDs correctly; `quen→queen`, `bohemain→bohemian`, `freddei→freddie` suggestions). Phase 4.4's public search page consumes these endpoints. (`VPerson`/`People_Overview` established the projection pattern.)
- **2.6 ✅ Likes** — one `UserLike` document per user (id `UserLikes/{userId}`, two id arrays `Likes`/`Dislikes` spanning all three subject collections) instead of per-(user,subject) rows, so "my favorites" is a single doc load and a toggle is an atomic single-doc write. Per-subject totals come from `Likes_Count : AbstractMultiMapIndexCreationTask<LikeCount>` — a **fan-out map-reduce** (one map over `Likes`, one over `Dislikes`, reduce sums by `SubjectId`). Custom anonymous-read `SubjectController` (`GET /api/subject/likes?id=` totals + the caller's own state; `POST /api/subject/likes` `{subjectId,like}` where `like` = true/false/null sets/clears, `[Authorize]`, waits for non-stale index so returned totals are fresh; `GET /api/subject/favorites` resolves liked ids via the shared `Subjects_Search`→`VSubject` projection). Verified end-to-end (like→count 1, switch to dislike, clear, cross-collection favorites Queen+Bowie). Subject ids are RavenDB string ids (contain `/`) so they travel as query/body params, not route segments. **CSRF note:** the cookie-auth POST has no antiforgery check yet — fine mid-migration; the hardened public surface is the JWT-bearer API (Phase 6.6, CSRF-immune). Like widget UI is Phase 4.3.
- **2.7 Media player-type derivation** — port `SongHelper.GetPlayerInfos` regex logic as a computed/projection field.

**Net-new renderers this phase:** ✅ color-swatch; ✅ multi-reference picker (became a **native Spark feature** — list-of-references + searchable multi-select, used for tags across Person/Artist/Song). Remaining: tag-tree, drag-reorder list. Reusable; upstream candidates.

**Exit criteria:** Full catalog manageable in admin UI; unified search/suggest works; likes work. _(Status 2026-06-08: 2.1–2.6 done — Person/Artist/Song + Medium/Tag/TagCategory CRUD, relationships, sub-queries, multi-reference tags, program-unit menus all live + verified against RavenDB; unified cross-type search + typo-tolerant suggest live (`/api/search`, RavenDB multi-map index — D1 confirmed, Elasticsearch droppable); likes/favorites live (`UserLike` per-user doc + `Likes_Count` fan-out map-reduce + `/api/subject/likes|favorites`). Left: 2.7 player-type.)_

---

## Phase 3 — Playlists, lyrics, blog — ~2–3 weeks

- **3.1 Playlist** — ordered tracks (AsDetail `{SongRef, Index}` + drag-reorder renderer), public/private with `IsAllowedAsync` row-level security, my/public scoped queries.
- **3.2 Lyrics** — per-user-per-song collection (`UserLyrics/{userId}/{songId}` id), `Text` + karaoke `double[]` timing; row-level access.
- **3.3 Blog** — `BlogPost` entity, `Administrator`/`Blogger` group rights, public read.
- **3.4 LogEntry** (optional) — or replace with structured logging.

**Exit criteria:** Playlists/lyrics/blog manageable and queryable with correct authorization.

---

## Phase 4 — Bespoke public site — ~4–6 weeks (largest hand-built effort)

Goal: the public-facing UX, reusing the `@mintplayer/ng-*` component family.

- **4.1 Global player shell** — root-level `<app-floating-player>` (draggable card, `@mintplayer/ng-video-player` + YouTube/Vimeo/DailyMotion/SoundCloud plugins), `<app-playlist-queue>` sidebar, root `PlayerService` (signals: queue, currentTrack, isPlaying, progress).
- **4.2 Karaoke** — real-time synchronized lyrics display in the player; the **lyrics-sync editor** (full-screen timestamp editor) consuming the Lyrics API.
- **4.3 Public detail pages** — artist/person/song (custom layout, embedded player, like widget, share buttons, SEO meta + JSON-LD per entity), consuming `SparkService`. SSR/render per D2.
- **4.4 Search page** — cross-entity results + autocomplete against the Phase-2.5 endpoints.
- **4.5 Home, public playlists, blog reading, GDPR** pages.
- **4.6 Design system** — public SCSS theme; scope ng-bootstrap admin theme separately; shared global player layer.

**Exit criteria:** Public site reaches feature parity with current bespoke pages; SEO acceptance test passes.

---

## Phase 5 — Full auth experience — ~3–4 weeks (parallelizable with Phase 4)

Backends mostly exist; this is largely net-new Angular + provider wiring.

- **5.1** Email-confirmation page; password-change page; profile/account shell.
- **5.2** **2FA enrollment** — QR display, enable/verify, recovery-code generation/display, disable; per-user "bypass 2FA for external login".
- **5.3** **Social login** — backend `AddGoogle`/`AddMicrosoftAccount`/`AddFacebook`/`AddGitHub` (+ Twitter/LinkedIn third-party if in v1 per D4); frontend social buttons + popup/redirect flow; `external-providers` endpoint.
- **5.4** **Account linking** — `/manage-logins` page (add/remove external logins).
- **5.5** **Passkeys/WebAuthn** (per D3 — **in v1, full parity**) — port the Fido2NetLib backend + register/list/remove + passwordless-login Angular components; carry over the `WebAuthnCredential` data. Build to be upstreamable into `ng-spark-auth`.
- **5.6** **Security fixes** — hash 2FA recovery codes, protect OAuth tokens (Spark `UserStore.cs`); contribute upstream.
- **5.7** Public signed-JWT API (only if D5 = keep).

**Exit criteria:** Full account suite functional in staging; security fixes merged.

---

## Phase 6 — Cross-cutting infra — ~2–3 weeks (parallelizable)

- **6.1 Search indexing** — finalize per D1 (RavenDB-native, or `IRecipient<SubjectIndexMessage>` → external engine).
- **6.2 Fetcher/Crawler** — port the 9 scrapers + container; host via `ISparkCronJob` (periodic) + `IRecipient<FetchMessage>` (event-driven); `web/v3/fetcher` → custom endpoint.
- **6.3 SEO endpoints** — sitemap (XML, video/image, hreflang), robots.txt, AMP song page, OpenSearch descriptor — as custom controllers, paths excluded from SPA fallback.
- **6.4 Email** — durable transactional email via messaging + MailKit.
- **6.5 Background durability** — confirm Spark Messaging replaces the DB job queue (retry/backoff/dead-letter/checkpoint).
- **6.6 Public REST API (D5)** — hand-written `api/v1/*` `ApiController`s injecting `IAsyncDocumentSession`, guarded by a dedicated JWT bearer scheme; a login endpoint mints signed JWTs via `UserManager<SparkUser>`; Swagger doc. Project documents to response shapes inline (no DTO/repo/service layer).

**Exit criteria:** Search, scraping, SEO endpoints, and email all functional and durable.

---

## Phase 7 — Data migration & cutover — ~2 weeks

- **7.1** Finalize the `MintPlayer.Migration` tool — full per-entity mapping, deterministic IDs, reconciliation report (see the **Migration tooling** section below for the design).
- **7.2** Dry-run against a production SQL snapshot; reconcile row counts; spot-check; verify existing-password login (D8 hash migration) and that migrated 2FA users can complete TOTP login with their existing authenticator.
- **7.3** Full feature-checklist regression in staging (every Section-3 item).
- **7.4** Cutover (single swoop): take old app offline → full SQL backup → run the ETL against the static snapshot → deploy the new app in its place → smoke test → monitor. No dual-run, no sync. If it fails the smoke test, redeploy the old build and restore the backup.

**Exit criteria:** Production fully on Spark/RavenDB in one replacement; old app retired; SQL backup retained for the rollback window.

---

## Phase 8 — Decommission & cleanup — ~1 week

- Delete `MintPlayer.Data`, `MintPlayer.Data.Abstractions`, `MintPlayer.Dtos`, the dual controller stacks, and the EF migrations.
- Remove Elasticsearch infra (if D1 = drop).
- Update CI (add `RAVENDB_LICENSE`), docs, and the deployment runbook.
- Retire SQL Server after the rollback window closes.

**Exit criteria:** Boilerplate layers gone (G1/G2/G5 met); single clean Spark codebase.

---

## Migration tooling — `MintPlayer.Migration` (dedicated deliverable)

The SQL→RavenDB migration is a build item in its own right, not a script written at the end. Because the cutover is a single offline swoop (D7), it can be a straightforward batch tool — but it must map every entity correctly and be re-runnable.

### Shape

A standalone .NET 10 console app that:
- **References the old `MintPlayer.Data`** to *read* the source — reuse the existing EF Core `MintPlayerContext` and entity classes rather than hand-writing SQL. Reads from a static snapshot/backup, not the live DB.
- **References the new Spark domain project** + a RavenDB `IDocumentStore` to *write* documents (use `BulkInsert` for throughput on large collections).
- Runs once, offline, with structured logging and a final reconciliation report.

### ID strategy — deterministic IDs (recommended)

Assign RavenDB IDs derived from the old SQL primary keys: `artists/{oldId}`, `people/{oldId}`, `songs/{oldId}`, `tags/{oldId}`, `playlists/{oldId}`, etc. This makes the tool **idempotent/re-runnable** and lets any cross-reference be computed directly from the old FK — no two-pass lookup map needed. Keep an `OldId` field on each document for traceability. (Fallback: a two-pass import with an in-memory `(type, oldId) → ravenId` map if deterministic IDs are undesirable.)

### Per-entity mapping (the actual work)

| Source (SQL/EF) | Target (RavenDB) | Notes |
|---|---|---|
| `Subjects` (TPH, discriminator) | `Artists` / `People` / `Songs` collections | Split by discriminator; carry audit/timestamp fields. |
| `ArtistPerson` (Active), `ArtistSong` (Credited) | embedded AsDetail arrays on `Artist` (or junction docs) | Preserve the `Active` / `Credited` business flags. |
| `SubjectTag` | `TagIds` array on the subject | Resolve to `tags/{oldId}`. |
| `Tag` (ParentId tree), `TagCategory` (Color ARGB) | `Tag` + `TagCategory` collections | Self-reference → `tags/{parentOldId}`; ARGB int → color value. |
| `Medium` + `MediumType` (Visible) | embedded media array + `MediumType` collection | Keep `Visible` flag. |
| `PlaylistSong` (Index) | ordered `Tracks` array on `Playlist` | Preserve track order via `Index`. |
| `Playlist` (IsDeleted / accessibility) | `Playlist` | Public/private; soft-delete policy (carry flag or skip). |
| `Lyrics` (Timeline `int[]×20` JSON) | `Lyrics` collection | Convert timeline → native `double[]`, preserve ÷20 scaling. |
| `Like` (DoesLike) | `Like` collection | Resolve subject ref across the 3 collections. |
| `BlogPost`, `LogEntry` | corresponding collections | Soft-delete policy. |
| `AspNetUsers` (+ `AspNetUserRoles`) | `SparkUser` | Copy `PasswordHash` + `SecurityStamp` **verbatim** (D8 — same default PBKDF2 hasher, no reset). Map roles → groups. Carry `PictureUrl`, `Bypass2faForExternalLogin`. |
| `AspNetUserTokens` (`AuthenticatorKey`, `RecoveryCodes`) + `TwoFactorEnabled` | `SparkUser.AuthenticatorKey` / `.TwoFactorRecoveryCodes` / `.TwoFactorEnabled` | **Verbatim** — preserves working TOTP (see Phase 7 + PRD §6). |
| `AspNetUserLogins` | `SparkUser.Logins` | External/social account links. |
| `WebAuthnCredentials` | passkey collection / `SparkUser` | Carry credential blobs so passkeys keep working. |

### Soft-delete & verification

- Decide per entity whether soft-deleted rows are imported (carry `IsDeleted`/`DeletedAt`) or skipped — align with the soft-delete convention from Phase 1.
- Produce a per-collection reconciliation: source row count (filtered) vs. documents written; fail loud on mismatch.

### Where it appears in the plan

- **Phase 0 spike 0.4** — prove the approach on one aggregate + a 2FA round-trip.
- **Phase 7.1** — finalize the full tool; **7.2** dry-run + reconcile; **7.4** run it during the cutover.
- Lives in its own project in the repo (per D6); deleted or archived post-cutover once no longer needed.

---

## Sequencing & parallelism

```
Phase 0  ──▶ Phase 1 ──▶ Phase 2 ──▶ Phase 3 ─┐
                                              ├─▶ Phase 7 ──▶ Phase 8
              Phase 4 (public UI) ────────────┤
              Phase 5 (auth)     ────────────┤
              Phase 6 (infra)    ────────────┘
```

Phases 4, 5, 6 can run in parallel once the domain (Phases 2–3) is stable. Phase 7 needs all of them.

---

## Rough effort

| Phase | Estimate |
|-------|----------|
| 0 — Spikes | 1–2 wks |
| 1 — Foundation | 2 wks |
| 2 — Catalog | 3–4 wks |
| 3 — Playlists/lyrics/blog | 2–3 wks |
| 4 — Public UI | 4–6 wks |
| 5 — Auth | 3–4 wks |
| 6 — Infra | 2–3 wks |
| 7 — Migration/cutover | 2 wks |
| 8 — Cleanup | 1 wk |
| **Total (with 4/5/6 partly parallel)** | **~16–22 weeks** |

---

## Biggest watch-items

1. **Passkeys (R2)** — in v1 (D3), and the single largest net-new auth item; allow schedule slack in Phase 5.
2. **Search quality (R4)** — validate RavenDB full-text on real data in Phase 0 before deleting Elasticsearch.
3. **Custom renderers** — multi-reference picker, tag-tree, drag-reorder, karaoke editor are recurring net-new UI; build them reusable and early.
4. **Public API auth wiring** — keep the JWT bearer scheme cleanly separated from Spark's cookie/Identity auth so default-scheme resolution doesn't break the SPA; validate in Phase 0 spike 0.6.
5. **Prerendering wiring (R1, resolved)** — low risk now via `MintPlayer.AspNetCore.SpaServices.Prerendering`, but still confirm `SparkService` is SSR-safe and admin chunks are excluded from prerender (Phase 0).
