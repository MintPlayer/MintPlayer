# PRD + Plan — Finish the MintPlayer → Spark migration and cut over

**Status:** Draft (2026-09-27)
**Author:** Pieterjan De Clippel (with Claude)
**Builds on:** [`PRD-Spark-Migration.md`](./PRD-Spark-Migration.md) (decisions D1–D8 still stand unless amended below), [`Implementation-Plan-Spark-Migration.md`](./Implementation-Plan-Spark-Migration.md), [`PRD-Feature-Parity.md`](./PRD-Feature-Parity.md), [`PRD-Player-Playlist.md`](./PRD-Player-Playlist.md)

This document **supersedes** the "Phase 6.2 Fetcher/Crawler" item and the "Migration tooling — `MintPlayer.Migration`" section of the implementation plan; both were stale against the current code (see §3.3 and §5).

---

## 1. Summary

The rewrite on `feature/spark-migration` (63 commits, Spark `10.0.0-preview.41`, ng-spark `^22.0.8`) has the catalog, playlists, likes, search, lyrics/karaoke and the global player done. What is left is **everything between "admin catalog works" and "mintplayer.com runs on Spark"**: the public site (SSR/SEO), the auth long tail, the public `api/v1`, the data-migration tool, and the cutover itself.

The single goal of this document: **migrate the entire website to Spark and cut over.** Scraping is not part of it — the legacy Fetcher/Crawler projects were deleted (§2.3).

## 2. Scope

### 2.1 Goals

| # | Goal |
|---|------|
| C1 | Close every remaining feature gap so the Spark app is a full replacement for the legacy site (§4). |
| C2 | Build `MintPlayer.Migration`, a repeatable, verifiable SQL Server → RavenDB migration (§5). |
| C3 | Rehearse the migration on a production snapshot in staging, then cut over in one swoop (D7 unchanged). |
| C4 | Delete `legacy/` after cutover. |

### 2.2 Delivery rule

All app work lands on `feature/spark-migration` → **one PR**.

Framework work in `MintPlayer.Spark` is split in two, by the user's decision (D32):
1. **Spark PR 1 — everything MintPlayer needs**: the composable-contributor seam (row policies + lifecycle interceptors), `MintPlayer.Spark.SoftDelete`, `MintPlayer.Spark.History` + ng-spark History panel, the security fixes (F7), #189, and whatever S1/S3/S9 surface. Merged and published to nuget.org/npm, then consumed by MintPlayer. **The cutover depends on this PR only.**
2. **Spark PR 2 — `MintPlayer.Spark.Moderation`** (D27), built on PR 1's packages after it merges. Not on the MintPlayer critical path.

### 2.3 Removed from scope (done 2026-09-27)

Deleted from `legacy/`: all 9 `Fetcher/*` sites + `Fetcher.Abstractions`/`Fetcher.Test`, `Crawler/*` (incl. the dead `Crawler.Data`), `MintPlayer.Fetcher.Integration`, the unused .NET API clients `MintPlayer.Client` / `MintPlayer.RestClient` / `.Test`, the `web/v3/fetcher` controller + view model, and their `Startup`/csproj/sln wiring.
Rationale: only Genius was ever registered in production, the UI entry point was commented out, and a liveness check (2026-09-27) found 2 of 9 sites still parse (AZLyrics captcha, SongLyrics Cloudflare, Musixmatch/Songteksten/SongMeanings/Lyrics.com redesigned, Muzikum never implemented). Nothing in the new app depends on them.

### 2.4 Out of scope

- **Any scraper / metadata extractor**, including the MintPlayer.AI-based one. Investigation notes are preserved in Appendix A so the idea can be picked up later as its own project; it is not being built here.
- Running old and new side by side, or any SQL↔RavenDB sync (D7).

---

## 3. Findings that shape the plan

### 3.1 Production data is small (exact numbers: §6.1)

Anonymous `GET https://mintplayer.com/api/v1/*` (with `Accept: application/json` and header `include_relations: true`) returned: **141 songs** (254 media, 127 with lyrics, 19 with karaoke timelines), **138 artists**, **9 persons**, 26 tags, 6 tag categories, **10 blog posts**, 2 public playlists (73 tracks), plus at least one private playlist. Consequences:
- Migration runtime is seconds; the search-at-prod-volume benchmark (R4) is trivial.
- Correctness, not throughput, is the risk: every row can be reconciled individually.

### 3.2 The API is not a sufficient migration source

The API cannot deliver users (hashes, 2FA, logins, roles, passkeys), per-user likes, private playlists, lyrics history/authorship, soft-deleted rows or audit columns. It also hides 8 medium types (ids 5, 7, 9, 11, 12, 15, 16, 18) from non-admins — **the genius/musixmatch links, if stored, are media of those hidden types**; an admin JWT exposes them.
→ **Primary source: a SQL Server backup (`.bak`) of production.** Supplied 2026-09-27 and profiled in S2 (§6.1). The API is used only as a cross-check and for dev seeding (`scripts/seed-catalog.mjs` already does this).

### 3.3 Spark state (per phase of the original plan)

Done: 0.3–0.5, 1.x, 2.x, 3.1, 3.2 (redesigned), 4.1, 4.2, 6.1.
**Not done:** 0.1 SSR spike (prerendering never wired), 0.6 JWT API in-repo, 3.3 Blog, 3.4 LogEntry, 4.3 public detail pages (partial), 4.4 search page, 4.5 home/public playlists/blog/GDPR, 4.6 theme, 5.x auth long tail, 6.3 SEO endpoints, 6.4 durable email (partial), 6.5 jobs, 6.6 `api/v1`, 7, 8. Also: Docker image never built, CI lacks `RAVENDB_LICENSE`, no antiforgery on `POST /api/subject/likes`, Spark #189 (inline AsDetail reference picker) open.

---

## 4. Remaining feature work

| # | Item | Notes |
|---|------|-------|
| F1 | **Blog** | New `BlogPost` entity (Title, Headline, Body, Author → user, Published) — 10 live posts must migrate; `Blogger` group already exists. Public list + detail page. |
| F2 | **Public site shell + SSR** | Wire `MintPlayer.AspNetCore.SpaServices.Prerendering` (D2) after spike S1: `main.server.ts`, `OnSupplyData` for subject pages, `UseSpaPrerendering`. |
| F3 | **Public detail pages** | Song/Artist/Person/Playlist/Tag public pages (not the admin PO page), with per-entity JSON-LD (`application/ld+json`, ng-seo 22.0.1) + OG tags. |
| F4 | **Search page + home** | UI over the existing `/api/search` + `/suggest`; home, public playlists, favorites, GDPR/privacy page, theme (4.6). |
| F5 | **SEO endpoints** | `sitemap.xml`, `robots.txt`, OpenSearch description. AMP is dropped; `/amp/song/{id}` 301-redirects to `/song/{id}` (D11). |
| F6 | **Auth long tail** | Email-confirm page, change password, profile; 2FA enrollment + recovery-code regeneration; social login for the five providers legacy registers — Facebook, Microsoft, Google, Twitter, LinkedIn (`legacy/MintPlayer.Web/Startup.cs:83-87`); GitHub (D4) is net-new, not parity  + account linking; passkeys (D3). Build upstreamable into `ng-spark-auth`. |
| F7 | **Spark security fixes (R7)** | Recovery codes are already hashed in `UserStore.HashRecoveryCode` — verify; protect stored OAuth tokens (R2-M11). Spark PR. |
| F8 | **Public `api/v1` (D5)** | Hand-written `[ApiController]`s over `IAsyncDocumentSession` + `AddJwtBearer` scheme; same routes/DTO shapes as legacy (Swagger at `/swagger/v1/swagger.json` is the contract). Keep `include_relations` header semantics. |
| F9 | **Durable email + jobs** | Move mail sending onto Spark Messaging (retry/dead-letter). No periodic jobs remain (ES indexing and scraping are gone) — confirm and close 6.5. |
| F10 | **Domain gaps for migration** | `MediumType.Visible` (hidden types must stay hidden to non-admins); `BlogPost` (F1). |
| F11 | **Hardening** | Registration rate-limiting + captcha (bot sign-ups, D18); Antiforgery on cookie-auth custom POSTs (`/api/subject/likes`, `/api/playlist/*`, `/api/song/lyrics/timings`); Spark #189; Docker image + CI `RAVENDB_LICENSE` secret. |
| F12 | **Soft delete on the row filter** | Target: adopt `MintPlayer.Spark.SoftDelete` (D28). Spark master (`5ebfaa45`, 2026-08-28) removed `OnLoadAsync(session,id)` / `OnQueryAsync(session)` that the app's soft delete relies on. Move it to `GetRowFilterAsync(action) => x => !x.IsDeleted` (applies to list/detail/edit/delete/stream/breadcrumb) and drop the hand-written filters in `EntityActions`, `MintPlayerSparkContext`, `ArtistActions`, `SongActions`. **Must land before upgrading past `preview.41`.** |
| F13 | **Listen-together rooms** (new) | A host opens a room from a playlist or the queue; guests join by link; play/pause/seek/next synced over a WebSocket (ASP.NET Core WebSockets + Spark `SocketExtensions` JSON helpers); guests add songs to the shared queue and vote to skip; host can hand over control. Room state in RavenDB (survives redeploy), live state in memory. Spike S9. |
| F14 | **Collaborative playlists + discovery** (new) | Owner invites co-editors (invite link / username) who may add, remove and reorder tracks — enforced with Spark row-level `IsAllowedAsync` ("owner or collaborator"); follow/like public playlists; public *Discover* page (most-liked, recently updated, by tag). |
| F15 | **Resume anywhere + listening history** (new) | Per-user server-side queue + position (`PlayerStates/{userId}`), restored on any device; *Recently played* and simple personal stats (top artists/songs) from a `PlayEvents` collection + map-reduce index. |
| F16 | **Import a YouTube playlist** (new) | Paste a YouTube playlist URL → YouTube Data API v3 `playlistItems.list` → match each video to an existing song by its YouTube `Medium` (canonical video id) → create a MintPlayer playlist; unmatched videos listed, optionally created as draft songs. API key in the server `.env`. Spike S10. |

---

## 5. `MintPlayer.Migration`

### 5.1 Shape

.NET 10 console app in `MintPlayer.slnx`.

```
Sources/   ILegacySource (IAsyncEnumerable per table + Capabilities flags)
           SqlSnapshotSource  — Dapper + Microsoft.Data.SqlClient over a restored .bak   (primary, complete)
           PublicApiSource    — /api/v1 with include_relations (catalog only; cross-check + dev seed,
                                replaces scripts/seed-catalog.mjs)
Model/     neutral legacy records (no EF)
Transform/ pure functions, unit-tested
Target/    RavenWriter (BulkInsert, explicit ids, app's Newtonsoft settings + Spark Color converter,
           compare-exchange writes, deploy indexes and wait for non-stale) · JsonlWriter (dry run)
Verify/    Reconciler
```

**Why not reference the legacy EF context** (as the old plan said): `MintPlayerContext` and its entities are `internal`, global query filters hide soft-deleted rows, value converters silently rewrite the Timeline (×20) and Color, and it drags in NEST, Fido2 and five OAuth packages. Plain SQL over the `ModelSnapshot` schema is simpler and exact.

Flags: `--dry-run | --run | --verify-only`, `--source sql|api|composite`, `--tz <prod timezone>`, `--migrate-passkeys` (off). `--run` wipes and recreates the target database (also avoids piling up Song revisions). Expected runtime: well under a minute plus index build.

### 5.2 Mapping

Ids: `{Collection}/{legacyId}` exactly as `seed-catalog.mjs` already does (`Artists/12`, `People/3`, `Songs/40`, `Tags/7`, `TagCategories/2`, `MediumTypes/1`, `Playlists/9`, `BlogPosts/4`); users `MintPlayerUsers/{legacyGuid:D}`; every `Entity` gets `OldId`. Audit: `CreatedAt←DateInsert`, `ModifiedAt←DateUpdate`, `DeletedAt←DateDelete`, `IsDeleted ← UserDeleteId references an existing user` — legacy semantics of the `UserDelete == null` navigation filter; zero GUID and a bare `DateDelete` are live (§6.1); `DateInsert = 0001-01-01` → fallback; local `DateTime.Now` values converted to UTC via `--tz`.

| Legacy | Target | Transform |
|---|---|---|
| `Subjects` TPH `artist` | `Artists/{id}` | Name, YearStarted, YearQuit; `Members` from `ArtistPerson` `{PersonId, Active}` |
| `Subjects` `person` | `People/{id}` | FirstName, LastName, Born/Died → `DateOnly` |
| `Subjects` `song` | `Songs/{id}` | Title; `Released` → `DateOnly?` (`0001-01-01` → null); `Artists` from `ArtistSong` `{ArtistId, Credited}` |
| any other discriminator | — | **fail** |
| `Media` | `Subject.Media[]` | ordered by Id, `{Value, TypeId}`; orphans (null SubjectId) reported + skipped |
| `SubjectTag` | `Subject.TagIds` | dedup |
| `MediumTypes` | `MediumTypes/{id}` | Description → Name; Visible → **Visible (F10)** |
| `Tags` / `TagCategories` | same | ParentId/CategoryId refs (`0` → null); cycle check; ARGB → Color (report alpha ≠ 255) |
| `Lyrics(SongId, UserId, Text, Timeline, UpdatedAt)` | `Song.Lyrics` + `Song.LyricsTimings` | one text per song (the 4 duplicates are identical — D12); timeline = the most complete non-empty one; `\n` endings; Timeline JSON `int[]` ÷ 20.0 — one entry per **non-blank** line, so re-index onto the `StartTimes` line layout and pad partial syncs with null; keyed to the song's first playable medium. |
| `Likes(SubjectId, UserId, DoesLike)` | `UserLikes/{userId}` | group per user into `Likes[]` / `Dislikes[]` |
| `Playlists` + `PlaylistSong` | `Playlists/{id}` | Name ← Description; `IsPublic = Accessibility==1`; OwnerId; Tracks ordered by Index (duplicates kept) |
| `BlogPosts` | `BlogPosts/{id}` | F1 |
| `LogEntries`, `Jobs` | — | dropped (ES queue / logs) |
| `AspNetUsers` | `MintPlayerUser` | scalars 1:1; **PasswordHash + SecurityStamp verbatim** (D8) |
| `AspNetUserRoles` | `Roles[]` | role names (`Administrator`, `Blogger`) = `security.json` group names |
| `AspNetUserLogins` / `Claims` | `Logins[]` / `Claims[]` | 1:1 |
| `AspNetUserTokens` | `AuthenticatorKey`, `TwoFactorRecoveryCodes`, `Tokens[]` | AuthenticatorKey verbatim (TOTP keeps working); recovery codes split on `;` and **SHA-256 lower-hex hashed** to match Spark `UserStore.HashRecoveryCode` (amends the "verbatim" note in D8) |
| — | compare-exchange `emails/{normalizedEmail}` → userId | required by Spark's email lookup/uniqueness |
| `WebAuthnCredentials` | — | table does not exist in production (§6.1); nothing to migrate (D13) |

Lost by design: authorship columns (`UserInsertId/UpdateId/DeleteId`), legacy `Media.Id`, TagCategory alpha, `ConcurrencyStamp`.

### 5.3 Pre-flight and verification

Pre-flight (fail the run): unknown discriminators, orphan FKs, tag cycles, duplicate normalized emails (legacy enforced uniqueness in code only), lyrics on non-songs.
Reconciler: per-table vs per-collection counts; every `*Id` reference resolves; SHA-256 of each transformed doc vs the doc re-read from RavenDB; N random side-by-side diffs; cross-check the public catalog against `PublicApiSource` (admin JWT, so hidden media types compare too); smoke login for a password user and a password+TOTP user through `SignInManager`.

---

## 6. Spikes

Each spike ends in a written result appended to this doc (numbers, decision, or both).

| # | Spike | Timebox | Exit criterion |
|---|---|---|---|
| S1 | **SSR/prerender** — wire `SpaServices.Prerendering` into MintPlayer.Web for one song page with `OnSupplyData` | 2 d | `curl` of `/song/{id}` returns rendered title, OG tags and JSON-LD without JS; no hydration errors; works in the Docker image.  |
| S2 | **Prod data profiling** on a restored `.bak` | 0.5 d | Table of row counts; WebAuthn rows; duplicate emails; alpha≠255; `Released=MinValue`; orphan media; lyrics versions per song; non-empty claims/tokens; hidden medium types and what they contain; server timezone. Every "probably empty" resolved. |
| S3 | **Identity round-trip** — migrate one password-only and one password+TOTP+recovery-codes user, log in via the running app | 1 d | Both log in; an existing authenticator code works; a legacy recovery code redeems; Administrator gets admin rights. |
| ~~S4~~ | **Cancelled** — no passkeys in production (§6.1). Was: passkey feasibility — can a legacy Fido2 credential (user handle = `Guid.ToByteArray()`, no backup flags) authenticate through Identity passkeys + Spark `UserStore`? | 1 d | Working mapping, or D13 confirmed (re-enrol + a notice email to affected users). |
| S5 | **End-to-end catalog migration** on the S2 snapshot | 1 d | Reconciler green, 0 unresolved references, 10 random subjects match legacy in the new UI. |
| S6 | **Lyrics timing** conversion | 0.5 d | 5 migrated songs highlight within 0.05 s of legacy; mismatch rate reported. |
| S7 | **`api/v1` compatibility** — replay recorded legacy responses (Swagger contract + the 8 catalog GETs) against the new controllers | 1 d | Field-level diff empty except documented deltas; JWT login works with the new user store. |
| S8 | **Hetzner deploy** — the cutover also moves hosting from FoxXL Plesk (IIS + SQL Server 2019) to the Hetzner VPS (D14). Port the CodeCoverage pipeline: workflow, prod compose, Traefik labels for `mintplayer.com`, Node 22 in the runtime image, license one-shot, health checks | 1.5 d | A staging hostname on the VPS serves the image deployed by the workflow; one SSR'd page works in the container; a RavenDB backup is restored into a scratch database. |
| S9 | **Listen-together sync** — two browsers in one room over a WebSocket through Traefik, YouTube + SoundCloud players | 2 d | Drift stays < 1 s over a 10-minute session incl. an ad/buffer stall on one client; reconnect after an app redeploy rejoins the room at the right position. |
| S10 | **YouTube import** — Data API quota + matching | 0.5 d | Import of a 100-video playlist uses < 5 quota units per 50 items; ≥ 90 % of videos that exist in the catalog are matched by canonical video id. |

A prod `.bak` was supplied on 2026-09-27 (see S2 results). S1–S8 can all proceed.

### 6.1 S2 result — production data profile (2026-09-27)

Source: native backup `mintplay_MintPlayer_2026-09-27_21-09-04` from `WEB22\MSSQLSERVER2019` (Plesk, SQL Server 2019, 10 MB), restored to LocalDB as `MintPlayer_Snapshot_20260927` (files in `C:\Users\piete\SqlData\MintPlayerSnapshot`). Aggregates only; no personal values were read out.

**Schema facts the tool must honour**
- Tables live in schema **`mintplay`**, not `dbo` → schema name is a `SqlSnapshotSource` setting.
- Last applied migration is **`20240627124318_xxx`**. Production never received `AddWebAuthnCredentials` or the .NET 10 migrations → **there is no `WebAuthnCredentials` table: no passkeys exist to migrate (D13 closed)**. Target the snapshot schema, not the repo's latest `ModelSnapshot`.
- **Soft delete is encoded two ways.** `Subjects` use `UserDeleteId = NULL` for live rows; `MediumTypes`, `Tags`, `TagCategories`, `BlogPosts` use the **zero GUID** (`00000000-…`) for live rows. Legacy's query filter is `UserDelete == null` on the *navigation*, so the rule that reproduces it is: **`IsDeleted = UserDeleteId joins an existing AspNetUsers row`** — the zero GUID and a bare `DateDelete` both count as live. Verified: this yields exactly the live counts the public API serves (138 artists, 9 persons, 141 songs, 12 visible medium types). A naive `UserDeleteId IS NOT NULL` would drop every tag, category, medium type and blog post.
- `Tags.ParentId = 0` and `Tags.CategoryId = 0` mean "none" → map to null (27 tags have ParentId 0; tags 41, 42 have CategoryId 0). All non-zero parents resolve; max depth 1; no cycles.
- 8 subjects have `DateInsert = 0001-01-01` → `CreatedAt` falls back to `DateUpdate`, else the migration timestamp.
- 2 `Media` rows have an empty `Value` → skip + report.

**Counts** (live / soft-deleted)

| Data | Rows | Notes |
|---|---|---|
| Artists | 145 (138 / 7) | |
| Persons | 15 (9 / 6) | |
| Songs | 141 (141 / 0) | one song has `DateDelete` set but no deleting user — live in legacy, so live after migration (keep `DeletedAt` null; report it) |
| Media | 507 | 6 on deleted subjects; 0 orphans; hosts: YouTube 225, Wikipedia 183, official sites, Vimeo 4, Dailymotion 3, **genius.com 5** |
| MediumTypes | 15 (13 / 2) | ids 5, 9, 11, 12, 16 hard-deleted; **type 15 "Songteksten" is `Visible = 0` and holds the 5 genius.com links** (3 artists, 2 songs). No musixmatch links exist. |
| Tags / TagCategories | 51 (49 / 2) / 8 (6 / 2) | 3 categories have alpha ≠ 255 |
| SubjectTag / ArtistSong / ArtistPerson | 132 / 154 (9 uncredited) / 11 | 0 orphans |
| Lyrics | 145 rows, 141 songs | 4 songs have 2 versions; 14 empty texts; LF only |
| Karaoke timelines | 20 | stored ×20 (`[336,490,…]` → 16.8 s …); **one entry per non-blank lyric line** (18 exact; songs 129 and 291 only partially synced: 12/63 and 1/63) |
| Playlists / tracks | 16 (11 live: 9 private, 2 public; 5 deleted) / 116 | 3 duplicate (playlist, song) pairs — keep; all owners exist |
| Likes | 156 likes, 0 dislikes | from only **3 users** |
| BlogPosts | 10 (all live) | 2020-05 → 2023-05; all have an author |
| Users | **752** | 722 Identity-v3 hashes, 30 social-only; 100 email-confirmed; only **109 show any activity** (confirmed email, like, playlist or external login); 0 duplicate emails/usernames |
| Roles | Administrator 1, Blogger 1 | no claims at all |
| External logins | Google 26, Facebook 4, Twitter 2, Microsoft 1, **LinkedIn 1** | all five are legacy providers (D17) |
| 2FA | 2 users enabled; 13 authenticator keys; 2 recovery-code sets | 11 keys belong to users who never finished enrolment |
| Jobs | 629 `elasticsearch` jobs, status 0 | drop |
| LogEntries | 0 | drop |

**Open from S2:** server timezone of the Plesk host (not in the backup) — assume `Europe/Brussels` unless told otherwise.

---

## 7. Phases

| Phase | Content | Depends on |
|---|---|---|
| P0 | Cleanup (done: §2.3) · update stale plan docs | — |
| P1 | Spikes S1, S3, S5–S10 (S2 done, S4 cancelled) · decisions recorded | — |
| P2 | Soft delete → `GetRowFilterAsync` (F12), domain gaps (F10), Blog (F1), hardening (F11), Spark PR (F7, #189, anything S1/S3/S4 needs) | P1 |
| P3 | Public site: SSR shell (F2), detail pages (F3), search/home/GDPR/theme (F4), SEO endpoints (F5) | S1 |
| P4 | Auth long tail (F6) — parallel with P3 | S3, S4 |
| P5 | `api/v1` (F8), durable email (F9) — parallel with P3 | S7 |
| P5b | Interactive features: collaborative playlists + discovery (F14), resume + history (F15), YouTube import (F16), listen-together rooms (F13) — parallel with P3 | S9, S10 |
| P6 | `MintPlayer.Migration` full build (§5) | S2, S5, S6 |
| P7 | Staging rehearsal: restore fresh `.bak` → migrate → reconcile → full regression (Playwright + visc-style smoke) → fix → repeat until clean | P2–P6 |
| P8 | Cutover (D30): `app_offline.htm` maintenance page on legacy → final `.bak` → migrate → verify → switch IIS/DNS to the Spark app. Rollback = redeploy legacy + restore `.bak` (D7) | P7 |
| P9 | Decommission: delete `legacy/`, SQL Server, Elasticsearch; drop EF/NEST/Fido2 deps | P8 + soak period |

Tests run once, at the end of each phase's implementation, not per milestone.

---

## 8. Decisions

Carried over: D1 (RavenDB search), D2 (SpaServices prerendering), D3 (passkeys in v1), D4 (social logins), D5 (keep `api/v1`), D6 (repo layout), D7 (single cutover, no sync), D8 (hashes verbatim).

| # | Decision | Proposed | Status |
|---|---|---|---|
| D9 | Migration source | SQL `.bak` primary (supplied 2026-09-27); public API only for cross-check + dev seed | **Decided 2026-09-27** |
| D10 | Scraping | Removed; Fetcher/Crawler deleted; no replacement in this project | **Decided 2026-09-27** |
| D11 | AMP pages | Drop; `301` `/amp/song/{id}` → `/song/{id}` (AMP gives no search preference since the 2021 page-experience update; Core Web Vitals + SSR + JSON-LD cover SEO) | **Decided 2026-09-27** |
| D12 | Lyrics history | Resolved by data (S2): the 4 songs with 2 versions have identical text; take that text and the **most complete non-empty timeline** (not "latest" — song 291's only timeline is on the older row). No revisions needed | **Decided 2026-09-27** |
| D13 | Passkeys | Nothing to migrate — production has no WebAuthn table | **Closed 2026-09-27** |
| D14 | Prod hosting of RavenDB + app | **Same setup as Spark `apps/CodeCoverage`**: GHCR image built by a GitHub workflow → SSH deploy (`appleboy/ssh-action`, `VPS_*` secrets) to the Hetzner VPS → Traefik (`web` network, Let's Encrypt) → compose with pinned `ravendb/ravendb:7.1.x` on an internal network + one-shot license-activation container + server-side `.env`. Differences: runtime image **needs Node 22** for SpaServices prerendering (D2; CodeCoverage has none), `USER app`, `/health` endpoints | **Decided 2026-09-27** |
| D15 | `MediumType.Visible` | **Keep**: add `Visible` to `MediumType`; media of invisible types are hidden from viewers without a new `ViewHiddenMedia` right (Administrator/Moderator). Type 15 "Songteksten" (the 5 genius links) migrates as hidden — kept as the source-URL dedup key for future scraping | **Decided 2026-09-27** |
| D20 | Cutover bar | **Full parity (G3, D3, D4 re-affirmed)** — passkeys ship in v1 although production has none | **Decided 2026-09-27** |
| D21 | Outgoing mail | Own `boky/postfix:v4.3.0` container in the MintPlayer stack, same config as CodeCoverage; `ALLOWED_SENDER_DOMAINS=mintplayer.com`; HELO stays the VPS PTR name `coverage.mintplayer.com` (one IPv4 → one PTR); IPv4 only; new DKIM key + `mail._domainkey.mintplayer.com` TXT; VPS IPv4 added to the `mintplayer.com` SPF record before the first send (DMARC `sp=reject`) | **Decided 2026-09-27** |
| D22 | DNS at cutover | Same setup as `coverage.mintplayer.com`: keep the zone where it is, repoint `mintplayer.com`/`www` A records to the VPS; lower TTL to 300 s a day before (rollback = repoint) | **Decided 2026-09-27** |
| D23 | RavenDB backups | Built-in **periodic backup task** configured from the app at startup (like `RevisionsConfigurator`): hourly incremental + nightly full to a VPS volume, plus an off-box copy (Hetzner Storage Box / S3-compatible — target still to pick; if the license tier forbids cloud destinations, local backup + cron `rsync`); 30-day retention; one restore test before cutover (S8) | **Decided 2026-09-27** |
| D24 | Durable mail (F9) | `IEmailSender<MintPlayerUser>` / `ISparkLinkConfirmationSender` only **publish** `{TemplateName, Language, To, Data}` on Spark Messaging; an `IRecipient` resolves the MJML template, fills it with `Data`, renders MJML→HTML and hands it to the `mintplayer-smtp` Postfix. Links travel inside `Data` (tokens expire ≤ 1 day, die on use); dead-letter cutoff < token lifetime. Stack: Mjml.Net + Scriban | **Decided 2026-09-27** |
| D25 | Mail template storage | MJML files in the repo, `MintPlayer.Web/Email/Templates/{name}.{lang}.mjml` (en/nl/fr), embedded resources; Scriban strict-variables on; a unit test renders every template × language with sample data so a missing field fails the build | **Decided 2026-09-27** |
| D26 | Editing model | **Open editing + safety net (parity)**: signed-in members create/edit catalog items; delete = soft delete; every change is a revision stamped `ModifiedBy`; **Moderator** group views history/diff, reverts, restores, locks, hard-deletes. Built as a reusable Spark package (revision endpoints, opt-in soft-delete convention with Restore/Purge rights, audit stamping, ng-spark History panel) in the single Spark PR | **Decided 2026-09-27** |
| D27 | Generic moderation | Ship **`MintPlayer.Spark.Moderation`** (+ ng-spark UI) in a **second Spark PR after PR 1 merges** (D32), designed after StackOverflow (reputation from votes, privilege thresholds, flags + review queues, moderator tools, audit log). MintPlayer is not required to adopt it; a Spark demo app uses it end to end and its E2E tests are the spec. Needs its own PRD in the Spark repo, drafted **before Spark PR 1 merges**, so the seam and History/SoftDelete APIs are checked against it before they are published | **Decided 2026-09-27** |
| D28 | Spark package layout | New **core seam: composable, DI-registered contributors — no package ships a base actions class** (an app cannot derive from two). (1) **Row policies** (`IRowPolicy`): row filters and `IsAllowedAsync` AND-combined with the actions class overrides; per-request cache; zero cost for types a policy does not apply to. (2) **Lifecycle interceptors** (`IPersistentObjectInterceptor`): before/after save, delete (may replace the delete — soft delete), load; run in registration order around the actions class hooks. On top: **`MintPlayer.Spark.SoftDelete`** (`ISoftDeletable`, delete interception, Restore/Purge rights, show-deleted bypass), **`MintPlayer.Spark.History`** (revisions from the model, `ModifiedBy` stamping, revision/revert endpoints, ng-spark History panel), **`MintPlayer.Spark.Moderation`** (depends on both). Seam validated against the moderation PRD draft before Spark PR 1 merges | **Decided 2026-09-27** |
| D29 | Lyrics model | **One shared `Song.Lyrics` text + History revisions** (revert/diff) instead of legacy per-user versions; karaoke timings stay keyed per medium. Intentional deviation from the legacy data model — parity is of capability (edit, recover), not of storage | **Decided 2026-09-27** |
| D30 | Cutover window | **Maintenance page on legacy** (`app_offline.htm` uploaded to the Plesk site) for ~30–60 min at a quiet hour → final `.bak` → migrate + reconcile → repoint DNS. No legacy code change. Rollback window ≈ 1 h after the switch; after that, writes on the new site make rollback lossy | **Decided 2026-09-27** |
| D31 | Listen-together access (F13) | Anyone with the link may **listen** (anonymous, display name); only **signed-in** users may add songs or vote to skip; only signed-in users host. Unguessable room ids, host can close the room to new listeners, rooms expire after inactivity, caps: 50 listeners/room and a per-host room limit | **Decided 2026-09-27** |
| D32 | Spark delivery | Two Spark PRs: PR 1 ships seam + SoftDelete + History (+ F7, #189) and is merged/published before the MintPlayer cutover; PR 2 ships Moderation afterwards. Explicit exception to the one-PR rule, decided by the user | **Decided 2026-09-27** |
| D16 | Editor / Moderator group | Rename **Editor → Moderator** (History/SoftDelete rights: history, diff, revert, restore, lock, purge); only member at migration: the current Administrator (Administrator ⊇ Moderator). No moderator mails, no new-account throttle — revisions are the safety net. `--editor-emails` flag dropped | **Decided 2026-09-27** |
| D17 | LinkedIn login | Keep — legacy registers it (`AspNet.Security.OAuth.LinkedIn`, `Startup.cs:83-87`); migrates like any external login | **Decided 2026-09-27** |
| D18 | Inactive accounts (643 of 752 show no activity) | Migrate all 752; **no migration announcement mail** (protects the shared VPS IP reputation) | **Decided 2026-09-27** |
| D19 | Timezone of legacy `DateTime.Now` values | `Europe/Amsterdam` (FoxXL is Dutch hosting; same offsets as Brussels) — verify in S5 by comparing a known recent edit time | Proposed |

## 9. Risks

| Risk | Mitigation |
|---|---|
| Snapshot drifts before cutover | The tool is re-run on a fresh `.bak` at P7/P8; the S2 queries (`docs/spikes/S2-data-profile/*.sql`, run with `sqlcmd -d <db> -i`) re-run as pre-flight. |
| SSR wiring harder than expected (never spiked) | S1 first; fallback is static prerender of public routes at build time. |
| Recovery codes / TOTP break after migration | S3 proves it end to end before P6. |
| Soft-delete misread (zero GUID) | Pre-flight asserts live counts match the public API (138 artists, 9 persons, 141 songs, 12 visible medium types). |
| Hidden medium types lose visibility semantics | D15 + reconciler checks visibility per type. |
| `api/v1` consumers break | S7 contract diff. |

---

## Appendix A — Scraper / MintPlayer.AI investigation (not being built)

Kept for a possible future project.
- **Legacy**: 9 regex-over-HTML fetchers behind `IFetcher{UrlRegex, Fetch}`; only Genius registered in prod; results were never saved automatically (`web/v3/fetcher` returned candidates matched by source URL stored as a `Medium`). Only fixtures: 7 Genius HTML pages.
- **MintPlayer.AI** (v0.7.0, net10.0, managed + optional ILGPU CUDA): tensors with autograd, MLP/ResidualMlp/conv nets, Adam, RL trainers; no tokenizer, embeddings, RNN/attention or generic supervised trainer. Feasible formulation: **DOM-node classification with an MLP** (hand-crafted per-node features → title/artist/album/date/lyrics/image/media-link/other), plus a second line-level classifier to clean lyrics (lyric / section header / noise). Needed framework additions: supervised trainer + P/R/F1, dropout, classifier checkpoint with feature-schema version, EmbeddingBag, predict facade.
- **Training data**: the prod catalog is too small (141 songs) for distant supervision; labels would come from sites' own structured blobs (JSON-LD, `__NEXT_DATA__`, Genius `__PRELOADED_STATE__`) matched to DOM nodes, evaluated leave-one-site-out.
- **Dedup key**: hidden medium types (D15) keep source URLs (e.g. the 5 genius links on type 15) so a future extractor can recognise already-imported documents, as legacy `GetByMedium` did.
- **Cascade**: structured metadata → trained net → optional LLM fallback on low confidence; cleaned, normalized output (split feat. artists, date precision, canonical media URLs, structured lyric sections).
