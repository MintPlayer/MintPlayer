# PRD + Plan — Finish the MintPlayer → Spark migration and cut over

**Status:** Draft (2026-09-27)
**Author:** Pieterjan De Clippel (with Claude)
**Builds on:** [`PRD-Spark-Migration.md`](./PRD-Spark-Migration.md) (decisions D1–D8 still stand unless amended below), [`Implementation-Plan-Spark-Migration.md`](./Implementation-Plan-Spark-Migration.md), [`PRD-Feature-Parity.md`](./PRD-Feature-Parity.md), [`PRD-Player-Playlist.md`](./PRD-Player-Playlist.md)

This document **supersedes** the "Phase 6.2 Fetcher/Crawler" item and the "Migration tooling — `MintPlayer.Migration`" section of the implementation plan; both were stale against the current code (see §6.1).

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

All app work lands on `feature/spark-migration` → **one PR**. Framework changes needed in `MintPlayer.Spark` are batched into **one Spark PR**, published, then consumed.

### 2.3 Removed from scope (done 2026-09-27)

Deleted from `legacy/`: all 9 `Fetcher/*` sites + `Fetcher.Abstractions`/`Fetcher.Test`, `Crawler/*` (incl. the dead `Crawler.Data`), `MintPlayer.Fetcher.Integration`, the unused .NET API clients `MintPlayer.Client` / `MintPlayer.RestClient` / `.Test`, the `web/v3/fetcher` controller + view model, and their `Startup`/csproj/sln wiring.
Rationale: only Genius was ever registered in production, the UI entry point was commented out, and a liveness check (2026-09-27) found 2 of 9 sites still parse (AZLyrics captcha, SongLyrics Cloudflare, Musixmatch/Songteksten/SongMeanings/Lyrics.com redesigned, Muzikum never implemented). Nothing in the new app depends on them.

### 2.4 Out of scope

- **Any scraper / metadata extractor**, including the MintPlayer.AI-based one. Investigation notes are preserved in Appendix A so the idea can be picked up later as its own project; it is not being built here.
- Running old and new side by side, or any SQL↔RavenDB sync (D7).

---

## 3. Findings that shape the plan

### 3.1 Production data is small

Anonymous `GET https://mintplayer.com/api/v1/*` (with `Accept: application/json` and header `include_relations: true`) returned: **141 songs** (254 media, 127 with lyrics, 19 with karaoke timelines), **138 artists**, **9 persons**, 26 tags, 6 tag categories, **10 blog posts**, 2 public playlists (73 tracks), plus at least one private playlist. Consequences:
- Migration runtime is seconds; the search-at-prod-volume benchmark (R4) is trivial.
- Correctness, not throughput, is the risk: every row can be reconciled individually.

### 3.2 The API is not a sufficient migration source

The API cannot deliver users (hashes, 2FA, logins, roles, passkeys), per-user likes, private playlists, lyrics history/authorship, soft-deleted rows or audit columns. It also hides 8 medium types (ids 5, 7, 9, 11, 12, 15, 16, 18) from non-admins — **the genius/musixmatch links, if stored, are media of those hidden types**; an admin JWT exposes them.
→ **Primary source: a SQL Server backup (`.bak`) of production.** No local copy exists; it must be taken on the IIS host. The API is used only as a cross-check and for dev seeding (`scripts/seed-catalog.mjs` already does this).

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
| F5 | **SEO endpoints** | `sitemap.xml`, `robots.txt`, OpenSearch description. AMP is dropped unless S1 shows it still matters (decision D11). |
| F6 | **Auth long tail** | Email-confirm page, change password, profile; 2FA enrollment + recovery-code regeneration; social login for every provider legacy configures (Google, Microsoft, Facebook, Twitter/X packages in `MintPlayer.Data.csproj`, plus GitHub per D4)  + account linking; passkeys (D3). Build upstreamable into `ng-spark-auth`. |
| F7 | **Spark security fixes (R7)** | Recovery codes are already hashed in `UserStore.HashRecoveryCode` — verify; protect stored OAuth tokens (R2-M11). Spark PR. |
| F8 | **Public `api/v1` (D5)** | Hand-written `[ApiController]`s over `IAsyncDocumentSession` + `AddJwtBearer` scheme; same routes/DTO shapes as legacy (Swagger at `/swagger/v1/swagger.json` is the contract). Keep `include_relations` header semantics. |
| F9 | **Durable email + jobs** | Move mail sending onto Spark Messaging (retry/dead-letter). No periodic jobs remain (ES indexing and scraping are gone) — confirm and close 6.5. |
| F10 | **Domain gaps for migration** | `MediumType.Visible` (hidden types must stay hidden to non-admins); `BlogPost` (F1). |
| F11 | **Hardening** | Antiforgery on cookie-auth custom POSTs (`/api/subject/likes`, `/api/playlist/*`, `/api/song/lyrics/timings`); Spark #189; Docker image + CI `RAVENDB_LICENSE` secret. |

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

Flags: `--dry-run | --run | --verify-only`, `--source sql|api|composite`, `--tz <prod timezone>`, `--editor-emails a,b`, `--migrate-passkeys` (off). `--run` wipes and recreates the target database (also avoids piling up Song revisions). Expected runtime: well under a minute plus index build.

### 5.2 Mapping

Ids: `{Collection}/{legacyId}` exactly as `seed-catalog.mjs` already does (`Artists/12`, `People/3`, `Songs/40`, `Tags/7`, `TagCategories/2`, `MediumTypes/1`, `Playlists/9`, `BlogPosts/4`); users `MintPlayerUsers/{legacyGuid:D}`; every `Entity` gets `OldId`. Audit: `CreatedAt←DateInsert`, `ModifiedAt←DateUpdate`, `DeletedAt←DateDelete`, `IsDeleted ← UserDeleteId≠null ∨ DateDelete≠null`; local `DateTime.Now` values converted to UTC via `--tz`.

| Legacy | Target | Transform |
|---|---|---|
| `Subjects` TPH `artist` | `Artists/{id}` | Name, YearStarted, YearQuit; `Members` from `ArtistPerson` `{PersonId, Active}` |
| `Subjects` `person` | `People/{id}` | FirstName, LastName, Born/Died → `DateOnly` |
| `Subjects` `song` | `Songs/{id}` | Title; `Released` → `DateOnly?` (`0001-01-01` → null); `Artists` from `ArtistSong` `{ArtistId, Credited}` |
| any other discriminator | — | **fail** |
| `Media` | `Subject.Media[]` | ordered by Id, `{Value, TypeId}`; orphans (null SubjectId) reported + skipped |
| `SubjectTag` | `Subject.TagIds` | dedup |
| `MediumTypes` | `MediumTypes/{id}` | Description → Name; Visible → **Visible (F10)** |
| `Tags` / `TagCategories` | same | ParentId/CategoryId refs; cycle check; ARGB → Color (report alpha ≠ 255) |
| `Lyrics(SongId, UserId, Text, Timeline, UpdatedAt)` | `Song.Lyrics` + `Song.LyricsTimings` | latest version per song; `\n` endings; Timeline JSON `int[]` ÷ 20.0, keyed to the song's first playable medium, padded to line count. Older versions → Song revisions (D12) |
| `Likes(SubjectId, UserId, DoesLike)` | `UserLikes/{userId}` | group per user into `Likes[]` / `Dislikes[]` |
| `Playlists` + `PlaylistSong` | `Playlists/{id}` | Name ← Description; `IsPublic = Accessibility==1`; OwnerId; Tracks ordered by Index (duplicates kept) |
| `BlogPosts` | `BlogPosts/{id}` | F1 |
| `LogEntries`, `Jobs` | — | dropped (ES queue / logs) |
| `AspNetUsers` | `MintPlayerUser` | scalars 1:1; **PasswordHash + SecurityStamp verbatim** (D8) |
| `AspNetUserRoles` | `Roles[]` | role names (`Administrator`, `Blogger`) = `security.json` group names; Editor from `--editor-emails` |
| `AspNetUserLogins` / `Claims` | `Logins[]` / `Claims[]` | 1:1 |
| `AspNetUserTokens` | `AuthenticatorKey`, `TwoFactorRecoveryCodes`, `Tokens[]` | AuthenticatorKey verbatim (TOTP keeps working); recovery codes split on `;` and **SHA-256 lower-hex hashed** to match Spark `UserStore.HashRecoveryCode` (amends the "verbatim" note in D8) |
| — | compare-exchange `emails/{normalizedEmail}` → userId | required by Spark's email lookup/uniqueness |
| `WebAuthnCredentials` | `Passkeys[]` | **default: not migrated, users re-enrol** (D13, spike S4) |

Lost by design: authorship columns (`UserInsertId/UpdateId/DeleteId`), legacy `Media.Id`, TagCategory alpha, `ConcurrencyStamp`.

### 5.3 Pre-flight and verification

Pre-flight (fail the run): unknown discriminators, orphan FKs, tag cycles, duplicate normalized emails (legacy enforced uniqueness in code only), lyrics on non-songs.
Reconciler: per-table vs per-collection counts; every `*Id` reference resolves; SHA-256 of each transformed doc vs the doc re-read from RavenDB; N random side-by-side diffs; cross-check the public catalog against `PublicApiSource` (admin JWT, so hidden media types compare too); smoke login for a password user and a password+TOTP user through `SignInManager`.

---

## 6. Spikes

Each spike ends in a written result appended to this doc (numbers, decision, or both).

| # | Spike | Timebox | Exit criterion |
|---|---|---|---|
| S1 | **SSR/prerender** — wire `SpaServices.Prerendering` into MintPlayer.Web for one song page with `OnSupplyData` | 2 d | `curl` of `/song/{id}` returns rendered title, OG tags and JSON-LD without JS; no hydration errors; works in the Docker image. Decide AMP (D11). |
| S2 | **Prod data profiling** on a restored `.bak` | 0.5 d | Table of row counts; WebAuthn rows; duplicate emails; alpha≠255; `Released=MinValue`; orphan media; lyrics versions per song; non-empty claims/tokens; hidden medium types and what they contain; server timezone. Every "probably empty" resolved. |
| S3 | **Identity round-trip** — migrate one password-only and one password+TOTP+recovery-codes user, log in via the running app | 1 d | Both log in; an existing authenticator code works; a legacy recovery code redeems; Administrator gets admin rights. |
| S4 | **Passkey feasibility** — can a legacy Fido2 credential (user handle = `Guid.ToByteArray()`, no backup flags) authenticate through Identity passkeys + Spark `UserStore`? | 1 d | Working mapping, or D13 confirmed (re-enrol + a notice email to affected users). |
| S5 | **End-to-end catalog migration** on the S2 snapshot | 1 d | Reconciler green, 0 unresolved references, 10 random subjects match legacy in the new UI. |
| S6 | **Lyrics timing** conversion | 0.5 d | 5 migrated songs highlight within 0.05 s of legacy; mismatch rate reported. |
| S7 | **`api/v1` compatibility** — replay recorded legacy responses (Swagger contract + the 8 catalog GETs) against the new controllers | 1 d | Field-level diff empty except documented deltas; JWT login works with the new user store. |
| S8 | **Production hosting** — where RavenDB and the app run after cutover (IIS host today at `C:\Inetpub\mintplayer.com`; RavenDB service vs container, license, backups) | 1 d | Deployment decision + a tested backup/restore of the RavenDB database (D14). |

Spikes S2–S6 need the prod `.bak` (user action: take it on the IIS host). S1, S7, S8 can start immediately.

---

## 7. Phases

| Phase | Content | Depends on |
|---|---|---|
| P0 | Cleanup (done: §2.3) · update stale plan docs | — |
| P1 | Spikes S1–S8 · decisions D9–D14 recorded | `.bak` for S2–S6 |
| P2 | Domain gaps (F10), Blog (F1), hardening (F11), Spark PR (F7, #189, anything S1/S3/S4 needs) | P1 |
| P3 | Public site: SSR shell (F2), detail pages (F3), search/home/GDPR/theme (F4), SEO endpoints (F5) | S1 |
| P4 | Auth long tail (F6) — parallel with P3 | S3, S4 |
| P5 | `api/v1` (F8), durable email (F9) — parallel with P3 | S7 |
| P6 | `MintPlayer.Migration` full build (§5) | S2, S5, S6 |
| P7 | Staging rehearsal: restore fresh `.bak` → migrate → reconcile → full regression (Playwright + visc-style smoke) → fix → repeat until clean | P2–P6 |
| P8 | Cutover: maintenance page on legacy → final `.bak` → migrate → verify → switch IIS/DNS to the Spark app. Rollback = redeploy legacy + restore `.bak` (D7) | P7 |
| P9 | Decommission: delete `legacy/`, SQL Server, Elasticsearch; drop EF/NEST/Fido2 deps | P8 + soak period |

Tests run once, at the end of each phase's implementation, not per milestone.

---

## 8. Decisions

Carried over: D1 (RavenDB search), D2 (SpaServices prerendering), D3 (passkeys in v1), D4 (social logins), D5 (keep `api/v1`), D6 (repo layout), D7 (single cutover, no sync), D8 (hashes verbatim).

| # | Decision | Proposed | Status |
|---|---|---|---|
| D9 | Migration source | SQL `.bak` primary; public API only for cross-check + dev seed | Proposed |
| D10 | Scraping | Removed; Fetcher/Crawler deleted; no replacement in this project | **Decided 2026-09-27** |
| D11 | AMP pages | Drop | Open (S1) |
| D12 | Lyrics history | Latest version into `Song.Lyrics`; older versions as Song revisions | Open |
| D13 | Passkeys | Not migrated; users re-enrol, notified by email | Open (S4) |
| D14 | Prod hosting of RavenDB + app | — | Open (S8) |
| D15 | `MediumType.Visible` | Keep (add to domain) | Proposed |
| D16 | Editor group membership | Passed via `--editor-emails`; none by default | Proposed |

## 9. Risks

| Risk | Mitigation |
|---|---|
| No prod backup available / can't restore locally | Take `.bak` on the IIS host first; restore to an mssql container. Everything in P6 is blocked without it. |
| SSR wiring harder than expected (never spiked) | S1 first; fallback is static prerender of public routes at build time. |
| Recovery codes / TOTP break after migration | S3 proves it end to end before P6. |
| Passkey users locked out | They also have a password or social login in legacy (verify in S2); D13 notice email. |
| Hidden medium types lose visibility semantics | D15 + reconciler checks visibility per type. |
| `api/v1` consumers break | S7 contract diff. |

---

## Appendix A — Scraper / MintPlayer.AI investigation (not being built)

Kept for a possible future project.
- **Legacy**: 9 regex-over-HTML fetchers behind `IFetcher{UrlRegex, Fetch}`; only Genius registered in prod; results were never saved automatically (`web/v3/fetcher` returned candidates matched by source URL stored as a `Medium`). Only fixtures: 7 Genius HTML pages.
- **MintPlayer.AI** (v0.7.0, net10.0, managed + optional ILGPU CUDA): tensors with autograd, MLP/ResidualMlp/conv nets, Adam, RL trainers; no tokenizer, embeddings, RNN/attention or generic supervised trainer. Feasible formulation: **DOM-node classification with an MLP** (hand-crafted per-node features → title/artist/album/date/lyrics/image/media-link/other), plus a second line-level classifier to clean lyrics (lyric / section header / noise). Needed framework additions: supervised trainer + P/R/F1, dropout, classifier checkpoint with feature-schema version, EmbeddingBag, predict facade.
- **Training data**: the prod catalog is too small (141 songs) for distant supervision; labels would come from sites' own structured blobs (JSON-LD, `__NEXT_DATA__`, Genius `__PRELOADED_STATE__`) matched to DOM nodes, evaluated leave-one-site-out.
- **Cascade**: structured metadata → trained net → optional LLM fallback on low confidence; cleaned, normalized output (split feat. artists, date precision, canonical media URLs, structured lyric sections).
