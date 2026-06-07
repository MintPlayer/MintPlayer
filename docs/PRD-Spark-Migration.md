# PRD — Rewrite MintPlayer on MintPlayer.Spark + RavenDB

**Status:** Draft for review
**Author:** Pieterjan De Clippel (with Claude)
**Date:** 2026-06-06
**Related plan:** [`Implementation-Plan-Spark-Migration.md`](./Implementation-Plan-Spark-Migration.md)

---

## 1. Summary

MintPlayer is today an ASP.NET Core MVC application using the repository pattern over EF Core + SQL Server, with Elasticsearch for search and a bespoke Angular 13 SPA (SSR + PWA). It is a public music catalog: artists, persons (band members), songs, tags, playlists, lyrics (with karaoke timing), likes, a blog, and a rich account system (local + 5 social providers, email confirmation, password reset, TOTP 2FA, WebAuthn passkeys).

This project rewrites MintPlayer on top of **MintPlayer.Spark** (`C:\Repos\MintPlayer.Spark`) — a low-code framework that replaces DTOs, repositories, and controllers with a generic `PersistentObject` middleware over **RavenDB**, plus a metadata-driven Angular 22 admin UI. The goal is to **prune the large boilerplate surface** (DTOs, mappers, repositories, services, dual controller stacks) while preserving every user-facing feature.

The bet is sound: Spark removes essentially all the CRUD/admin/data-access boilerplate and gives us auth, messaging, cron, indexing, and concurrency for free. The remaining hand-built work concentrates in areas Spark does **not** cover out of the box: **the bespoke public-facing UI** (global media player, song/artist pages, karaoke editor), and **the long tail of auth** (social login UI, 2FA enrollment, passkeys). SEO/SSR — originally the top risk — is handled by the existing **`MintPlayer.AspNetCore.SpaServices.Prerendering`** library, so it is wiring rather than net-new engineering.

---

## 2. Goals & Non-Goals

### 2.1 Goals

| # | Goal |
|---|------|
| G1 | Replace SQL Server + EF Core + repositories + services + DTOs + mappers with Spark entities over RavenDB. |
| G2 | Replace the dual `api/v1` (JWT) + `web/v3` (cookie) controller stacks with the generic Spark middleware for all CRUD. |
| G3 | Preserve **all** current user-facing functionality (catalog browse/search, media player, playlists, likes, lyrics + karaoke, blog, full account/auth suite, i18n, SEO). |
| G4 | Keep the public site's design and UX; reuse the existing `@mintplayer/ng-*` player/playlist component family. |
| G5 | Net reduction in hand-written code (target: eliminate the `MintPlayer.Data`, `MintPlayer.Data.Abstractions`, `MintPlayer.Dtos` repository/service/mapper layers). |
| G6 | One Angular app hosting both a metadata-driven **admin** area and the bespoke **public** site. |

### 2.2 Non-Goals

| # | Non-goal |
|---|----------|
| N1 | Re-platforming away from Angular (we stay on Angular, upgraded to 22 to match ng-spark). |
| N2 | Changing the product feature set or visual redesign (this is a re-platform, not a redesign). |
| N3 | Making Spark an OpenID Connect identity provider. |
| N4 | Migrating the standalone Crawler's scratch database; the crawler/fetcher logic is ported but its experimental storage is out of scope. |
| N5 | Multi-tenant or cross-module replication (Spark's Replication/Webhooks packages are not needed for a single-module app). |

---

## 3. Current-State Inventory (what must be preserved)

Condensed from the investigation. Full detail in the team reports.

### 3.1 Domain

- **Polymorphic `Subject`** (TPH in SQL, discriminator) → `Artist`, `Person`, `Song`. Shared: media, tags, likes, soft-delete, rowversion concurrency, audit timestamps.
- **Join entities carrying business data:** `ArtistPerson` (`Active` = current/past member), `ArtistSong` (`Credited` flag), `SubjectTag`, `PlaylistSong` (`Index` ordering), `Like` (`DoesLike`), `Lyrics` (per-user-per-song `Text` + karaoke `Timeline` array).
- **`Tag`** self-referencing tree under colored **`TagCategory`**.
- **`Medium`** (URL) typed by **`MediumType`** (`Visible` flag); player type (YouTube/Vimeo/DailyMotion/SoundCloud) derived by regex at read time.
- **`Playlist`** (public/private), **`BlogPost`** (role-gated), **`LogEntry`**, **`Job`/`ElasticSearchIndexJob`**.
- **ASP.NET Identity** (`User`/`Role` + 7 Identity tables), **`WebAuthnCredential`**.
- Soft-delete via global query filters; optimistic concurrency via manual rowversion Base64 compare.

### 3.2 API

- `api/v1/*` — public REST, **JWT bearer**, Swagger.
- `web/v3/*` — SPA backend-for-frontend, **cookie + X-XSRF-TOKEN**.
- Both cover: artist, person, song, playlist, subject (likes/search/suggest/favorites), mediumtype, tag, tagcategory, blogpost, account. Plus `amp/song/{id}`, `sitemap`, `robots.txt`, `/opensearch.xml`, `web/v3/fetcher`.
- Paging via `PaginationRequest<T>` with multi-column sort; header flags `include_relations`, `root_tags_only`.

### 3.3 Auth (the hard part)

Local login + registration, email confirmation, password reset, password change, **5 OAuth providers** (Facebook, Google, Microsoft, Twitter, LinkedIn; GitHub configured), account linking, **TOTP 2FA** + recovery codes + per-user "bypass 2FA for external login", **WebAuthn/passkeys** (Fido2NetLib), JWT + cookie, XSRF.

### 3.4 Frontend (Angular 13, SSR + PWA)

- Global shell with **floating draggable video player** + **playlist queue sidebar**, real-time synchronized **karaoke lyrics**, multi-platform playback.
- Bespoke public pages: home, search (cross-entity autocomplete), artist/person/song detail (SEO meta + JSON-LD), playlist show/public, blog, GDPR.
- CRUD/management screens for catalog + tags + medium types.
- Account pages: login (email/social/passkey), register, profile (password, social links, 2FA + QR, passkeys, backup codes), password reset, 2FA verify.
- i18n (en/fr/nl via ngx-translate), PWA service worker, SEO (titles, OpenGraph, hreflang, canonical), JSON-LD (`MusicGroup`/`MusicRecording`/`VideoObject`/`BlogPosting`), AMP song page.

### 3.5 Infrastructure

- Elasticsearch (NEST) full-text + completion suggest; SQL fallback when inactive; 5s job-queue indexer.
- Crawler + 9 scraper Fetchers (Genius, Musixmatch, AZLyrics, etc.) → metadata import.
- SMTP (registration/reset email), file-system Data Protection keys, sitemap/robots/AMP/OpenSearch SEO, request logging service (currently disabled).

---

## 4. Target Architecture

```
┌─────────────────────────── One Angular 22 app ───────────────────────────┐
│  Public shell (/)                        Admin shell (/admin, authz)       │
│  ├─ home, search, artist/song/person     ├─ ...sparkRoutes()  (auto CRUD)  │
│  ├─ playlist, blog, gdpr                  │   po/{type}, query/{id}         │
│  ├─ account (login/register/2fa/...)      └─ powered by @mintplayer/ng-spark│
│  └─ powered by hand-built components                                       │
│                                                                            │
│  Global overlay layer in app root (outside both shells):                   │
│   <app-floating-player/> <app-playlist-queue/> <app-karaoke-editor/>       │
│   backed by a root PlayerService (signals)                                 │
└────────────────────────────────────────────────────────────────────────────┘
                                   │  HTTP (cookie + X-XSRF-TOKEN; bearer for public API)
┌────────────────────────────────────────────────────────────────────────────┐
│  ASP.NET Core 10 host                                                       │
│  ├─ Spark generic middleware  (/spark/*)  — all CRUD, queries, actions       │
│  ├─ Spark auth (/spark/auth/*) — Identity API + OAuth + XSRF + groups        │
│  ├─ Custom controllers — sitemap, robots, amp, opensearch, fetcher, public API│
│  ├─ Cron jobs / message recipients — scraping, search indexing, email        │
│  └─ SparkContext over RavenDB                                               │
└────────────────────────────────────────────────────────────────────────────┘
        │                                   │
   RavenDB (documents + indexes      Optional external search engine
   + full-text + suggestions)        (only if RavenDB FT proves insufficient)
```

### 4.1 Stack

| Layer | Current | Target |
|-------|---------|--------|
| Backend | ASP.NET Core MVC + repositories | ASP.NET Core 10 + Spark middleware |
| Data | SQL Server + EF Core | RavenDB (Spark `SparkContext`) |
| Search | Elasticsearch (NEST) | RavenDB full-text + `SuggestUsing` (default); external engine only if needed |
| Identity | ASP.NET Identity (SQL) | Spark Authorization (`SparkUser` in RavenDB) + Identity API |
| Frontend | Angular 13 + Universal SSR + PWA | Angular 22, ng-spark admin + bespoke public site |
| Background | IHostedService + DB job queue | Spark Cron + Messaging + Subscription Workers |

---

## 5. Functional Requirements & Spark Fit

Legend: ✅ clean fit · 🟡 needs config/custom code on a Spark hook · 🔴 net-new build or framework gap.

### 5.1 Domain modeling

| Requirement | Fit | Approach |
|---|---|---|
| Artist / Person / Song as entities | ✅ | Three RavenDB collections, three `SparkContext` properties. Shared base class for common fields. |
| Cross-type unified search / favorites | 🔴 | `AbstractMultiMapIndexCreationTask<VSubject>` + a `Custom.` query method; no built-in polymorphic query. |
| Artist↔Person (`Active`), Artist↔Song (`Credited`) | ✅ | AsDetail arrays embedding `{ Ref + flag }`, inline edit; or junction collections for heavy queries. |
| Subject↔Tag (multi-select) | 🟡 | Store `string[] TagIds`; **multi-reference picker UI is a custom renderer** (no built-in array-of-references widget). |
| Tag self-referencing tree + TagCategory color | 🟡 | `[Reference(typeof(Tag))]` on `ParentId`; `Color` auto-maps to `dataType:color`. Tree **view** widget is custom. Children via sub-query on detail page. |
| Playlist ordered tracks, public/private | ✅/🟡 | AsDetail array with `Index`; row-level visibility via `IsAllowedAsync`. Drag-reorder is a custom renderer. |
| Per-user lyrics + karaoke timing `double[]` | ✅/🔴 | `Lyrics` collection stores cleanly; **timing-array editor is a custom renderer** (the karaoke sync UI). |
| Media URLs typed + Visible flag | ✅ | AsDetail array + `LookupReference` for MediumType. |
| Likes/dislikes + aggregate counts | ✅/🟡 | `Like` collection clean; aggregate counts need an index projection (no built-in aggregate attribute). |
| Soft-delete | 🟡 | Override `OnDeleteAsync` to flag + filter every query/index. No built-in pattern. |
| Optimistic concurrency | ✅ | Built in — `PersistentObject.Etag` (change vector), HTTP 409 on conflict. |
| Multi-language UI **and** data | ✅ | `culture.json` + `TranslatedString` (first-class data type with per-language merge on save). |

### 5.2 API

| Requirement | Fit | Approach |
|---|---|---|
| CRUD for all catalog entities | ✅ | Generic Spark middleware (`/spark/po/*`, `/spark/queries/*`). Eliminates both controller stacks. |
| Paging + multi-column sort + search | ✅ | Built into `QueryExecutor`. **Search push-down** to RavenDB is custom per searchable entity (default filters in memory). |
| Likes / favorites / suggest endpoints | 🟡 | Custom actions or minimal-API endpoints alongside Spark middleware. |
| Public REST API with **signed JWTs** | ✅/🟡 | **Confirmed feasible (D5 = keep).** Write plain `[ApiController]`s that inject `IAsyncDocumentSession` (or `IDocumentStore`) and query the same RavenDB data layer Spark CRUD uses — proven by `Demo/WebhooksDemo/.../GitHubProjectsController.cs` (injects `IAsyncDocumentSession`, runs under `[Authorize]`, coexists via `AddControllers()`/`MapControllers()`). Register `AddJwtBearer()` as a separate named scheme with a signing key; guard the public API with `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`. A small login endpoint mints the JWT via the `UserManager<SparkUser>`/`SignInManager` Spark already registers. **Nuance:** injecting the typed `SparkContext` into a controller does *not* auto-populate its `.Session` (internal setter, set only by Spark's pipeline) — inject the session directly. See §5.6. |
| AMP, sitemap, robots, OpenSearch, fetcher | ✅ | Plain MVC controllers/minimal-APIs coexist with Spark (proven in WebhooksDemo); exclude their paths from the SPA fallback. |
| XSRF (X-XSRF-TOKEN) | ✅ | Exact match — Spark uses `X-XSRF-TOKEN` cookie/header out of the box. |

### 5.3 Authentication & Authorization

| Capability | Fit | Notes |
|---|---|---|
| Local login + registration | ✅ | `AddSparkAuthentication` + `MapSparkIdentityApi` + `sparkAuthRoutes()`. |
| Password reset (forgot + reset) | ✅ | Backend + Angular components shipped. Needs `IEmailSender` wired. |
| Cookie session + XSRF | ✅ | Built in. |
| Account lockout, password rules | ✅ | `IdentityOptions`. |
| Group-based authorization | ✅ | `security.json` — per-entity, per-action rights, multilingual group names, hot-reload. |
| Row-level security (private playlists, own lyrics) | 🟡 | Override `IsAllowedAsync`/`OnQueryAsync` in Actions classes. |
| Email confirmation | 🟡 | Backend works; **confirmation page is custom Angular**. |
| Password change (logged-in) | 🟡 | Backend endpoint exists; **change-password page is custom Angular**. |
| TOTP 2FA — login verification | ✅ | `SparkTwoFactorComponent` shipped. |
| TOTP 2FA — enrollment (QR, enable/disable, recovery codes) | 🔴 | Backend endpoints exist; **enrollment UI is net-new Angular** (2–3 components). |
| Social OAuth (Google, Microsoft, Facebook) | 🟡 | Backend: chain `AddGoogle/AddMicrosoftAccount/AddFacebook` to the IdentityBuilder. **Login buttons + popup flow are net-new frontend.** |
| Social OAuth (GitHub) | ✅ | First-class `AddGitHub()` extension. |
| Social OAuth (Twitter, LinkedIn) | 🔴 | Need third-party NuGet packages (Twitter is OAuth2/PKCE now); frontend buttons net-new. |
| Account linking (add/remove logins) | 🔴 | Backend store methods exist; **`/manage-logins` page is net-new**. |
| WebAuthn / passkeys | 🔴 | **Not in Spark at all** — but **in scope for v1** (D3 = full parity). Port the existing Fido2NetLib backend integration + Angular register/list/remove/passwordless-login components. Largest single auth effort; build to be upstreamable into `ng-spark-auth`. |
| Per-user "bypass 2FA for external login" | 🔴 | Custom field on `SparkUser` subclass + custom logic. |
| Signed JWT issuance | 🟡 | See API section. |

**Known Spark security issues to fix before production** (from `PRD-SecurityAudit.md`): 2FA recovery codes stored plaintext (R2-M10); OAuth tokens stored plaintext (R2-M11). Both are small fixes in `UserStore.cs` and should be contributed upstream.

### 5.4 Frontend

| Requirement | Fit | Approach |
|---|---|---|
| Admin CRUD screens (catalog, tags, medium types, users) | ✅ | `...sparkRoutes()` auto-renders; near-zero code. |
| Public catalog detail pages (artist/song/person) | 🔴 | Hand-built Angular consuming `SparkService`; custom layout + SEO + JSON-LD. |
| Global floating player + playlist queue + karaoke editor | 🔴 | Hand-built root-level overlay components + root `PlayerService`; reuse `@mintplayer/ng-video-player` (already in the Spark monorepo). |
| Search page (cross-entity, autocomplete) | 🔴 | Hand-built; backed by custom search/suggest endpoints. |
| Blog reading, home, GDPR pages | 🔴 | Hand-built static/content pages. |
| Auth pages (login/register/reset/2fa-verify) | ✅ | ng-spark-auth shipped components, customizable via `sparkAuthRoutes(config)`. |
| Auth pages (profile/2fa-enroll/social/passkeys/email-confirm) | 🔴 | Net-new (see 5.3). |
| i18n (en/fr/nl) | 🟡 | Spark has its own translation system (not ngx-translate); public pages use `SparkLanguageService`. Migration of existing translation JSON required. |
| **SSR / prerendering** | 🟡 | ng-spark ships no SSR, but **`MintPlayer.AspNetCore.SpaServices.Prerendering`** already renders Angular via the `@angular/platform-server` boot module with per-route `OnSupplyData` data injection — same mechanism the current site uses (working demo at `C:\Repos\MintPlayer.AspNetCore.SpaServices\Demo\Prerendering`). Work is wiring `SparkService`/`OnSupplyData` to be SSR-safe, not building SSR from scratch. Risk R1 downgraded. |
| **PWA / service worker** | 🔴 | Not in ng-spark; re-add `@angular/service-worker` + `ngsw-config.json`. |
| SEO meta tags + JSON-LD + sitemap | 🔴 | No `Meta`/`Title`/JSON-LD support in ng-spark; re-add via Angular `Meta`/`Title` services and existing `@mintplayer/ng-json-ld`. Sitemap stays a server endpoint. |
| Design system coexistence | 🟡 | ng-spark is tied to `@mintplayer/ng-bootstrap`. Public site uses custom SCSS; scope admin theme separately. Two shells, shared global player layer. |

### 5.5 Infrastructure

| Requirement | Fit | Approach |
|---|---|---|
| Search indexing | ✅/🟡 | **Default: drop Elasticsearch**, use RavenDB `FieldIndexing.Search` indexes + `SuggestUsing`. If insufficient: feed an external engine via an `IRecipient<SubjectIndexMessage>` broadcast from `OnAfterSaveAsync`. |
| Search-as-you-type | 🟡 | Custom `Custom.` query methods using RavenDB `Search(x => x.Name, term + "*")`; one per searchable entity. |
| Crawler / Fetchers (scraping) | ✅/🔴 | Hosting fits: `ISparkCronJob` (periodic) + `IRecipient<FetchMessage>` (event-driven) + arbitrary `AddHostedService`. The scraper/parser code itself is ported as-is (net-new only in that it's lifted, not rewritten). |
| Transactional email (SMTP) | 🟡 | No Spark email abstraction; register MailKit/`IEmailSender` as a normal service, optionally drive via messaging for durable retry. |
| SEO endpoints | ✅ | Custom controllers coexist. |
| Background job durability | ✅ | Spark Messaging (retry, backoff, dead-letter, checkpoint) replaces the hand-rolled DB job queue. |
| Deployment | ✅ | Docker Compose (app + RavenDB) + Traefik; guide provided. Single-node Raven by default — cluster is separate infra. |
| Testing | ✅ | `SparkTestDriver` (embedded RavenDB) + `SparkEndpointFactory` + `SparkTestClient`. Needs `RAVENDB_LICENSE` in CI. |

### 5.6 Public REST API (signed JWT) over the Spark data layer

The current `api/v1/*` surface (public, JWT-authenticated, Swagger-documented) is **retained** for other instances/external consumers (D5 = keep). It does **not** go through the Spark PersistentObject middleware; instead we keep hand-written `ApiController`s that talk to the same RavenDB data layer.

Verified pattern (from `Demo/WebhooksDemo/.../GitHubProjectsController.cs`):

```csharp
[ApiController]
[Route("api/v1/artist")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public partial class ArtistApiController : ControllerBase
{
    [Inject] private readonly IAsyncDocumentSession _session;   // same Spark/RavenDB data layer

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
        => Ok(await _session.LoadAsync<Artist>(id));

    [HttpGet]
    public async Task<IActionResult> Page([FromQuery] int skip, [FromQuery] int take)
        => Ok(await _session.Query<Artist>().Skip(skip).Take(take).ToListAsync());
}
```

Wiring:
- `builder.Services.AddControllers()` + `endpoints.MapControllers()` (every Spark demo already does this); add `/api` to the SPA-fallback exclusion.
- Register a **separate** JWT scheme: `AddAuthentication().AddJwtBearer(o => { /* signing key, issuer, audience */ })`, alongside Spark's cookie/Identity auth (`AddSparkAuthentication`). No conflict — distinct scheme names, just like today's `api/v1` (JWT) vs `web/v3` (cookie).
- A small login endpoint mints the signed JWT using the `UserManager<SparkUser>`/`SignInManager` that Spark registers via Identity.
- Inject `IAsyncDocumentSession` (request-scoped) or `IDocumentStore` (singleton) — **not** the typed `SparkContext`, whose `.Session` is only populated inside Spark's own pipeline (internal setter). Reusing the typed context's queryables in arbitrary controllers would be a small, worthwhile upstream contribution to Spark.

This keeps the public API thin (no DTOs/repositories/services needed — query documents directly, project to response shapes inline) while the SPA itself uses the generic Spark middleware.

---

## 6. Data Migration

A one-time ETL from SQL Server → RavenDB is required (net-new tooling, run **once, offline**). Because the cutover is a single full-swoop replacement with no side-by-side running and no SQL↔RavenDB synchronization (D7), the migration is a straightforward batch job against a static source snapshot — no incremental sync, no change-data-capture, no dual-write. The `OldId` field below exists only to resolve references *within* the import; it is not used for any cross-system runtime lookup.

| Concern | Plan |
|---|---|
| Subjects → 3 collections | Split `Subjects` rows by discriminator into `Artists`/`People`/`Songs` documents; map int IDs → RavenDB string IDs; keep an `OldId` field for cross-reference resolution. |
| Join tables → embedded/junction | `ArtistPerson`/`ArtistSong`/`SubjectTag`/`PlaylistSong` become embedded AsDetail arrays (or junction docs), preserving `Active`/`Credited`/`Index` flags. |
| Lyrics timeline | The `int[]×20` JSON converts to native `double[]`; preserve scaling. |
| TagCategory.Color | ARGB int → color value. |
| Identity tables → SparkUser | Migrate users. **Password hashes migrate verbatim (D8)** — copy `AspNetUsers.PasswordHash` → `SparkUser.PasswordHash` and `SecurityStamp` → `SparkUser.SecurityStamp` (same default PBKDF2/Identity-V3 hasher on both sides; null hash for social-only users). Existing passwords keep working; Identity re-hashes to new parameters on next login if config differs. External logins and WebAuthn credentials migrate into the `SparkUser` document / passkey collection. 2FA continuity: see next row. |
| **2FA (TOTP) continuity** | **Existing authenticator codes keep working without re-enrollment** — both the old app and Spark use the identical ASP.NET Core Identity `AuthenticatorTokenProvider`; only the storage location differs. The ETL must copy, **verbatim and untransformed**: (1) the **authenticator secret** from `AspNetUserTokens` where `LoginProvider = '[AspNetUserStore]'` and `Name = 'AuthenticatorKey'` → `SparkUser.AuthenticatorKey`; (2) `AspNetUsers.TwoFactorEnabled` → `SparkUser.TwoFactorEnabled`; (3) recovery codes from `AspNetUserTokens` `Name = 'RecoveryCodes'` (`;`-separated) → `SparkUser.TwoFactorRecoveryCodes`. The otpauth issuer label is irrelevant to verification. **Note:** recovery codes are plaintext on both sides today (security item R2-M10); if the upstream hashing fix is applied, migrate codes accordingly or regenerate after cutover. The TOTP secret itself stays plaintext on both sides — a direct copy preserves working codes. |
| Soft-deleted rows | Carry `IsDeleted`/`DeletedAt`; do not import hard if policy is to purge. |
| Referential integrity | Two-pass import: create all documents, then resolve references by `OldId`. |
| Verification | Row-count reconciliation + spot-check queries against a static source snapshot. Take a full SQL backup before the run (rollback = redeploy old build + restore backup). Validate fully in staging before the production swoop. |

---

## 7. Risks

| ID | Risk | Severity | Mitigation |
|----|------|----------|------------|
| **R1** | ~~No SSR in ng-spark.~~ **Resolved by decision D2.** SEO via `MintPlayer.AspNetCore.SpaServices.Prerendering` (`@angular/platform-server` + `OnSupplyData`), the same approach the current site uses. | **Low-Med** | Phase-0 spike confirms `SparkService` works under server-side HTTP and `OnSupplyData` injects per-route data (titles, meta, JSON-LD) into Angular transfer state; ensure lazy admin chunks are excluded from prerender. |
| R2 | WebAuthn/passkeys absent from Spark; **in scope for v1** (D3). | Med-High | Port the existing Fido2NetLib backend + Angular components; allow extra schedule for it in Phase 5; contribute to `ng-spark-auth`. |
| R3 | Social login UI + account linking are net-new frontend. | Medium | Build a reusable social-button + popup component early; reuse existing `@mintplayer/ng-*` social login components where possible. Consider upstreaming to ng-spark-auth. |
| R4 | RavenDB full-text may underperform Elasticsearch for autocomplete at scale. | Medium | Benchmark with production data volume in Phase 1 spike. Keep the messaging-fed external-engine path as a fallback design. |
| R5 | Data migration correctness (polymorphism, references). | Medium | Build idempotent ETL with reconciliation; dry-run against a prod snapshot; keep SQL backup for rollback. **Password-hash risk low** — identical default PBKDF2 hasher on both sides; copy `PasswordHash`+`SecurityStamp` verbatim. **Proven in `spikes/Spike.Migration` (Phase-0 spike 0.4, 5/5 green):** verbatim-hash login, legacy lower-iteration hash → `SuccessRehashNeeded`, live TOTP code validates against the copied authenticator key, and TPH→3-collection reference resolution. |
| R6 | Angular 13 → 22 jump for ported bespoke components (player, karaoke). | Medium | Components depend on `@mintplayer/ng-*` libs at v22 in the Spark monorepo. Adopting ng-spark required realigning it to ng-bootstrap 22 (datatable merge + toggle→checkbox), shipped as `@mintplayer/ng-spark@22.0.0` (Spark PR #179) — expect similar small framework PRs when porting the player/karaoke. |
| R7 | Spark security issues (plaintext recovery codes/OAuth tokens). | Low-Med | Fix in `UserStore.cs` before production; contribute upstream. |
| R8 | Two design systems (ng-bootstrap admin vs custom public). | Low | Scope admin theme under the admin shell; share the global player layer. |

---

## 8. Success Criteria

1. All entities (Artist, Person, Song, Tag, TagCategory, MediumType, Medium, Playlist, Lyrics, Like, BlogPost, User) are modeled in RavenDB and CRUD-manageable through Spark.
2. The `MintPlayer.Data`, `MintPlayer.Data.Abstractions`, and `MintPlayer.Dtos` repository/service/mapper/DTO layers are deleted; controllers reduced to the SEO/AMP/fetcher/public-API set.
3. Every feature in Section 3 works end-to-end in staging, verified against a feature checklist.
4. Search + autocomplete return correct results within target latency on production-scale data.
5. SEO acceptance: public artist/song/person pages serve correct title/meta/JSON-LD to crawlers (mechanism per D2).
6. Full auth suite functional (or passkeys explicitly deferred with a tracked follow-up).
7. Data migration reconciles 100% of non-deleted records; users can log in with their existing passwords; external logins / 2FA / passkeys carry over.
8. Net hand-written LOC is materially lower than today (the boilerplate-pruning objective).

---

## 9. Decisions (resolved 2026-06-06)

| ID | Decision | Resolution |
|----|----------|------------|
| **D1** | Search engine. | **RavenDB full-text + `SuggestUsing`, benchmark first.** Validate quality/latency on prod-scale data in the Phase-0 spike before removing Elasticsearch. |
| **D2** | SEO/SSR strategy. | **Use `MintPlayer.AspNetCore.SpaServices.Prerendering`** — Angular rendered through Node via the `@angular/platform-server` boot module, data injected per-route with `OnSupplyData` (the current site's mechanism). Working reference: `C:\Repos\MintPlayer.AspNetCore.SpaServices\Demo\Prerendering`. SSR is **not** built from scratch; R1 downgraded. |
| **D3** | Passkeys/WebAuthn in v1. | **Full parity including passkeys.** Port the Fido2NetLib WebAuthn flow in v1 so the new site matches the current one exactly. |
| **D4** | Social providers in v1. | Google + Microsoft + Facebook + GitHub (low-effort backends). Twitter/LinkedIn included if their current OAuth2 packages integrate cleanly; otherwise fast-follow. |
| **D5** | Public signed-JWT API. | **Keep** — other instances need to query it. Implemented as hand-written `ApiController`s over `IAsyncDocumentSession` + a dedicated JWT bearer scheme (see §5.6). Confirmed feasible. |
| **D8** | Password migration. | **Migrate password hashes verbatim — no reset.** Both the old app and Spark use ASP.NET Core Identity's default `PasswordHasher<TUser>` (PBKDF2/Identity-V3, **not BCrypt**, no custom hasher), so the hash is portable. The ETL copies `AspNetUsers.PasswordHash` → `SparkUser.PasswordHash` and `SecurityStamp` → `SparkUser.SecurityStamp` (null hash for social-only users). Existing logins keep working; if the new app's hasher iteration count differs, Identity transparently re-hashes to the new parameters on the user's next successful login (forward-compatible). A one-time reset remains a fallback only. |
| **D6** | Repo. | **New structure inside `C:\Repos\MintPlayer`** (executed): legacy app moved to `legacy/` (deleted at cutover), new `MintPlayer.Web` Spark host + `MintPlayer.Domain` + `MintPlayer.slnx` at the root. Spark consumed via `ProjectReference` to the local clone during co-development; switch to versioned NuGet packages at release (Phase 6). |
| **D7** | Cutover. | **Single full-swoop replacement.** The old app and the new app never run side-by-side, and there is **no synchronization** between SQL Server and RavenDB. One offline migration run, then redeploy the new app in place of the old one. (No strangler, no dual-run, no live fallback.) Rollback, if needed, means redeploying the previous build against a retained SQL backup — a manual restore, not a live system. |

---

## 10. References

- Spark README, `docs/prd/PRD.md`, `docs/prd/PRD-AllFeatures.md`, `docs/Spark-API-Specification.md`
- Guides: reference/asdetail attributes, queries-and-sorting, aliases, translated-strings, attribute-grouping, custom-actions, custom-attribute-renderers, docker-deployment
- Auth: `libs/authorization/.../README.md`, `PRD-Authorization.md`, `PRD-login-page.md`, `PRD-custom-auth-routing.md`, `PRD-SecurityAudit.md`
- Demos: `Demo/Fleet`, `Demo/HR`, `Demo/DemoApp`, `Demo/WebhooksDemo`
