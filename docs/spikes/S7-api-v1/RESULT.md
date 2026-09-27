# S7 — public `api/v1` compatibility (2026-09-27)

Spike S7 of `docs/PRD-Spark-Completion.md` (F8, D5, §6). **Contract:** the live legacy API (`https://mintplayer.com/api/v1`,
Swagger `recorded/swagger_v1.json`) and the legacy controllers/mappers (`legacy/MintPlayer.Web/Server/Controllers/Api/V1`,
`legacy/MintPlayer.Data/Mappers`). **Data:** `MintPlayer_S7` = a fresh `MintPlayer.Migration --run` of the 2026-09-27 snapshot
(reconciler GREEN), plus one synthetic user. The app ran with `ASPNETCORE_ENVIRONMENT=Staging`, `ASPNETCORE_URLS=http://localhost:5401`,
`Spark__RavenDb__Database=MintPlayer_S7`.

| Exit criterion | Result |
|---|---|
| Field-level diff empty except documented deltas | **Met.** 39/39 cases pass (33 recorded responses, 1 derived reference, 5 status-only); 0 unexplained differences; 1 625 explained by deltas D1–D5 |
| JWT login works for a migrated user | **Met** with a synthetic user made to look migrated (user name ≠ e-mail, legacy V3 HMAC-SHA256/10k hash): login by e-mail **and** by user name → 200 + token; the legacy hash is rehashed on login |
| An authorized endpoint works with the token | **Met.** `account/current-user`, `account/roles`, `playlist/my`, a private `playlist/{id}`, `subject/{id}/likes` (own like) → 200 with the token; 401 without it or with a tampered token |

Probe: 22/22 checks pass (`apiv1_auth_probe.py`), including hidden medium types (D15) and the 2FA response. No real user's
credentials were used; no token, password or personal value was printed.

## What was built

`MintPlayer.Web/ApiV1/`:

```
ApiV1Dtos.cs            the legacy wire DTOs (V1Song, V1Artist, …), field for field
ApiV1Catalog.cs         per-request catalog: loads the live collections once, maps entities onto the DTOs,
                        reproducing the legacy mappers incl. derived fields (text, description, youtubeId…, playerInfos,
                        dateUpdate) and which relations each legacy EF query had loaded
ApiV1Jwt.cs             AddApiV1(): the "ApiV1Jwt" JwtBearer scheme, signing key, ApiV1TokenService
ApiV1ControllerBase.cs  caller resolution (JWT, else Spark cookie), include_relations header, User DTO mapping
CatalogV1Controllers.cs artist, person, song (+/{id}, /{id}/lyrics, /page), mediumtype, tag, tagcategory
UserV1Controllers.cs    account (login, current-user, roles), playlist (public, my, {id}), blogpost, subject/{id}/likes
```

`Program.cs` calls `builder.Services.AddApiV1(...)`; `MintPlayer.Web.csproj` adds `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.12.
Harness: `apiv1_diff.py` (field-level diff) + `recorded/` (the legacy responses, public anonymous data only), `apiv1_auth_probe.py`.

```powershell
# app on :5401 against the copy, then:
python docs\spikes\S7-api-v1\apiv1_diff.py            # exit 0 = no unexplained difference
python docs\spikes\S7-api-v1\apiv1_auth_probe.py      # writes ONLY to a *_S7 database; exit 0 = all pass
```

## Endpoints

All routes are case-insensitive, JSON only, ids are the legacy integers. Legacy ids map to `{Collection}/{oldId}` (§5.2).

| Route | Auth | Mirrors (legacy query) | Parity |
|---|---|---|---|
| `POST account/login` | anonymous | `AccountRepository.LocalLogin(createCookie: false)` | shape checked by the probe; see D8–D10 |
| `GET account/current-user` | JWT | `GetCurrentUser` (sensitive User DTO) | probe |
| `GET account/roles` | JWT | `GetCurrentRoles` → role-name strings | probe |
| `GET artist`, `GET artist/{id}` | anonymous | `GetArtists` / `GetArtist` (tags with category only on `/{id}`) | diff: list, list+rel, 22, 22+rel |
| `GET person`, `GET person/{id}` | anonymous | `GetPeople` / `GetPerson` | diff: list, list+rel, 170, 7+rel |
| `GET song`, `GET song/{id}` | anonymous | `GetSongs` / `GetSong` | diff: list, list+rel, 23, 23+rel, 6+rel, 404 |
| `GET song/{id}/lyrics` | anonymous | `SongController.Lyrics` | diff: 6, 23 (timed) |
| `POST song/page` | anonymous | `PageSongs` (no media/artists loaded → `playerInfos: []`, `description = title`) | diff vs. a reference **derived** from `songs.json` |
| `GET mediumtype`, `GET mediumtype/{id}` | anonymous | `GetMediumTypes` / `GetMediumType` (hidden → privileged only) | diff: list, 1, 404; probe: hidden 15 |
| `GET tag`, `GET tag/{id}` | anonymous | `GetTags` (`root_tags_only` header, default true) / `GetTag` | diff: list, list+rel, 2, 2+rel |
| `GET tagcategory`, `GET tagcategory/{id}` | anonymous | `GetTagCategories` / `GetTagCategory` | diff: list, list+rel, 1+rel |
| `GET playlist/public` | anonymous | `GetPlaylists(Public)` | diff: plain, +rel |
| `GET playlist/my` | JWT | `GetPlaylists(My)` | probe |
| `GET playlist/{id}` | public: anyone; private: owner (401 anonymous, 403 other) | `GetPlaylist` | diff: 9+rel, 16, 404; probe: private |
| `GET blogpost`, `GET blogpost/{id}` | anonymous | `GetBlogPosts` (newest first) / `GetBlogPost` | diff: list, 1 |
| `GET subject/{id}/likes` | anonymous (+ own like when signed in) | `SubjectService.GetLikes` over `Likes_Count` | diff: 6, 22, 23; probe: own like |

`include_relations: true` is honoured everywhere the legacy action had the header. The mapper reproduces the legacy EF
**relationship fix-up** artefacts because they are visible in the responses and cost a few lines each:

- nested songs of `GET artist` (+rel) list only the artists already iterated in their `description` (`"A Sky Full of Stars - Coldplay"`
  under Coldplay, `"… - Coldplay & Avicii"` under Avicii); under `artist/{id}` only that artist;
- a tag's category appears on nested tags only where the legacy query had the category loaded (`song/{id}` yes, `song` list no;
  child tags of `GET tag` when a root tag of the result has the same category);
- `tagcategory` (+rel) lists the tags whose parent was not loaded by the query.

Soft-deleted documents are invisible everywhere (legacy global filter `UserDelete == null`, `Playlist.IsDeleted`).
Media of hidden medium types (D15) are shown only to Administrator/Moderator (checked per request from the user document).

## Deltas

Diffed (each has a rule in `apiv1_diff.py`; count = differences it explains in the last run):

| # | Field | Legacy | New | Reason | Count |
|---|---|---|---|---|---|
| D1 | `*.concurrencyStamp` | base64 SQL `rowversion` | RavenDB change vector (opaque string) | rowversion not migrated (§5.2). Only meaningful for PUT, which v1 no longer serves | 1112 |
| D2 | `media[].id` | `Media.Id` | `0` | media are embedded, their SQL id is lost by design (§5.2) | 508 |
| D3 | `dateUpdate` of 2 artists | `0001-01-01T00:00:00` | snapshot as-of instant | S5 `created-fallback` (DateInsert 0001-01-01, no DateUpdate) | 2 |
| D4 | `media` of 2 artists | includes an entry with `value: ""` | entry absent | S5 skips empty media | 2 |
| D5 | `Songs/291` `lyrics.timeline` | `[]` (latest, untimed row) | the 1-entry timeline | D12 migrates the most complete timeline (S6) | 1 |

Behavioural (not reachable with anonymous recordings; by design):

| # | Where | Legacy | New |
|---|---|---|---|
| D6 | content negotiation | XML (DataContract) when no/other `Accept`; JSON on `Accept: application/json` | JSON always |
| D7 | `song/{id}/lyrics` for a missing song | 500 (null dereference) | 404 |
| D8 | `account/login` field `email` | e-mail only (`FindByEmailAsync`) | e-mail, then user name (S3: 750/752 users have `UserName ≠ Email`) |
| D9 | login with an unconfirmed e-mail | 500 (`EmailNotConfirmedException` uncaught) | 401 |
| D10 | JWT claims | `nameid` = user GUID, `unique_name`, `email`, HS256 | same names; `nameid` = the document id `MintPlayerUsers/{guid}`, plus `role` claims. Tokens are opaque to clients; old tokens are invalid anyway (new key) |
| D11 | `subject/{id}/likes`, `playlist/{id}`, hidden-media checks | only the Identity **cookie** identified the caller on these anonymous endpoints | JWT **or** cookie |
| D12 | hidden media privilege | `Administrator` role | `Administrator` or `Moderator` (role or `group` claim) — D15's future `ViewHiddenMedia` right |
| D13 | `playerInfos` / `youtubeId`… | computed over **all** media, hidden types included | computed over the media the caller may see (no difference today: the only hidden type, 15 "Songteksten", holds genius links) |
| D14 | `user.pictureUrl` | `""` | `""` — the migration stored null; the mapper writes `""` back (all 752 users are empty) |
| D15 | `song/page` bad `sortProperty` | 500 (dynamic OrderBy throws) | 400. Supported: `Id`, `Title`/`Text`/`Description`, `Released`, `DateUpdate`; `sortColumns` ignored (as legacy). Title sort uses invariant-culture ignore-case, close to but not guaranteed identical to the SQL collation |
| D16 | new documents | every row had an int id | a post-cutover document with a non-numeric id is exposed as `id: 0` — see open question 1 |

## JWT design

- A separate scheme **`ApiV1Jwt`** (`AddJwtBearer`) next to Spark's Identity cookie/bearer; never the default scheme, so Spark's
  own auth is untouched. Protected v1 actions use `[Authorize(AuthenticationSchemes = "ApiV1Jwt")]`; anonymous v1 actions call
  `AuthenticateAsync("ApiV1Jwt")` only when a `Bearer` header is present, else fall back to the cookie user.
- HS256, issuer/audience/lifetime/signing key validated, `MapInboundClaims = false`. Config section **`ApiV1:Jwt`**:
  `Issuer` (default `https://mintplayer.com/`), `Audience` (`Music`, as legacy), `ValidFor` (`02:00:00`, as legacy), `Key`
  (≥ 32 bytes; env `ApiV1__Jwt__Key`). **Production refuses to start without a key**; Development/Staging generate a random
  per-process key (tokens die with the process).
- `POST account/login`: `FindByEmailAsync` → `FindByNameAsync`; `SignInManager.CheckPasswordSignInAsync(lockoutOnFailure: true)`
  (so migrated SHA-256 hashes are rehashed, and failed attempts count toward lockout); unconfirmed e-mail → 401; a 2FA account gets
  `status: 2` and no token (v1 never had a second step); success → `{status: 1, user, error: null, errorDescription: null, token}`.
- Each request re-reads the user (`FindByIdAsync(nameid)`, lockout honoured) and its roles, so a role change or lock-out applies
  immediately; revoking a token before expiry would need a security-stamp claim (not done; legacy did not either).

## What consumers must change

Nothing for the read endpoints, provided they send `Accept: application/json` (they do) and treat `concurrencyStamp` as an opaque
string and `media[].id` as not meaningful. Users log in again once (new signing key). Clients that relied on XML, on the write
endpoints or on the favorites/search endpoints must wait for them (next section).

## Not built (explicitly out of the spike)

Write endpoints (`POST/PUT/DELETE` artist/person/song/tag/tagcategory/mediumtype/playlist/blogpost, `PUT song/{id}/timeline`,
`POST subject/{id}/likes`), `account/register`, `*/favorite`, `subject/favorite`, `subject/search` + `search/suggest`, the other
`*/page` endpoints, `playlist/my/page` + `public/page`, and serving `/swagger/v1/swagger.json`. The read-side patterns carry over:
each is a thin action over `ApiV1Catalog` (search should reuse `Subjects_Search`).

## Open questions

1. **Integer ids after cutover.** Documents created by the new app get Spark/RavenDB ids; v1 needs integers. Options: a
   `{Collection}/{n}` HiLo id convention for catalog types (keeps `LegacyId` working), or an `OldId`-like `PublicId` stamped on
   create. Until then new documents appear with `id: 0` and cannot be fetched by id.
2. **Write endpoints — keep or retire?** If kept, `concurrencyStamp` = change vector gives optimistic concurrency for PUT for free.
3. **Blog posts** read the stopgap `BlogPosts` collection through a local read model (`V1BlogPostDocument`); switch to the F1
   entity when it lands (field names already match).
4. **Per-request full-collection loads** (~1 200 documents; each catalog call does ≤ 6 queries). Fine for the current catalog and
   for a low-traffic compat API; move reverse lookups to static indexes and add output caching if traffic grows.
5. **`song/page` sort collation** is verified only against a derived reference (sorted by id); title sort vs. SQL collation was not
   compared live (the spike stayed with read-only GETs).
6. Should `Moderator` see hidden media (D12), or only the explicit `ViewHiddenMedia` right once it exists?

## Environment notes

- `MintPlayer_S7` also holds the synthetic user `s7-synthetic@example.invalid`, `Playlists/900001` and its `UserLikes` doc; it
  is a throwaway copy — re-running the migration recreates it.
- The repo's `MintPlayer.SourceGenerators` InjectSourceGenerator generates constructors for classes deriving from a base with
  constructor parameters (it collided with a primary constructor), so `ApiV1ControllerBase` resolves its services from
  `HttpContext.RequestServices` instead.
- Building with `-p:EnableSpaBuilder=false` skips the SPA; the NodeServices targets still rewrite `ClientApp/package*.json`
  (reverted, not committed).
