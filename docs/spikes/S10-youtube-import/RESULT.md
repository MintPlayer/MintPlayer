# S10 — YouTube playlist import: Data API quota + matching

**Date:** 2026-09-27 · **Feature:** F16 · **Verdict: both exit criteria met — GO.**

| Exit criterion | Target | Measured |
|---|---|---|
| Quota for a ~100-video playlist | < 5 units per 50 items | **5 units for 98 videos (2.55 / 50)**, 3 of them for the match itself; **0 units** for the follow-up import (cached) |
| Matching correctness | ≥ 90 % of playlist videos that exist in the catalog (by id) are matched | **100 %** — 22 / 22 across 7 real playlists (5,081 videos), 0 false positives; plus 164 / 164 catalog media URLs resolved through the index |

## What was built

| Piece | File |
|---|---|
| URL parsing: playlist id from any form, canonical video id from any form | `MintPlayer.Web/YouTube/YouTubeUrl.cs` |
| Data API client (`playlists.list`, `playlistItems.list` paging, `videos.list` batching). Key goes in the **`X-Goog-Api-Key` header**, never the URL | `MintPlayer.Web/YouTube/YouTubeDataApiClient.cs` |
| Pure matcher (order + duplicates kept, deterministic tie-break, case-sensitive) | `MintPlayer.Web/YouTube/YouTubePlaylistMatcher.cs` |
| Importer: preview, import, draft songs, 10-minute playlist cache | `MintPlayer.Web/YouTube/YouTubePlaylistImporter.cs` |
| Static index `Songs_ByYouTubeId` (RavenDB name `Songs/ByYouTubeId`) | `MintPlayer.Web/Indexes/Songs_ByYouTubeId.cs` |
| Endpoints | `MintPlayer.Web/Controllers/PlaylistImportController.cs` |
| 72 unit tests (URL variants, matcher, client paging/quota/key-not-in-URL/error handling) | `MintPlayer.Web.Tests/` |

Config: `YouTube:ApiKey` (env `YouTube__ApiKey`, i.e. the server `.env`) or `YouTube:ApiKeyFile` (a path to a file holding the key). Neither is set by default.

### Endpoints (signed-in users; `[Authorize]`, cookie scheme)

- `POST /api/playlist/import/youtube/preview {url}` returns `{playlistId, playlistTitle, totalVideos, matched[{position, videoId, videoTitle, songId, songTitle, otherSongIds}], unmatched[{position, videoId, title, channelTitle, duration, available, unavailableReason, blockedRegions}], apiCalls[], quotaUnits}`.
- `POST /api/playlist/import/youtube {url, name?, isPublic, createDraftsForUnmatched}` returns `{playlistId, name, trackCount, matchedTracks, draftSongsCreated, draftTracks, skippedVideos, preview}`. It creates a `Playlist` in YouTube order. `OwnerId` is set by the same `PlaylistActions.StampOwner` the PO pipeline uses (extracted into a static helper), so row-level security behaves the same as for a playlist made in the UI.
- Errors: 400 for a URL that isn't a playlist or for a mix (`RD…`), 401 anonymous, 403 when drafts are requested by a non-editor, 404 for a missing or private playlist, 429 when the quota is exhausted, 503 when no key is configured, 502 for other API errors.
- Antiforgery: none, the same as the app's other custom POSTs (`/api/subject/likes`, lyrics timings). The hardened surface is the Phase 6.6 bearer API.

## Quota: exact calls

Every Data API list method costs **1 unit per call**, whatever `part` asks for, including failed calls.

| Call | When | Per |
|---|---|---|
| `playlists.list` (part=snippet) | once per preview | playlist (title for the default name, and a clean 404) |
| `playlistItems.list` (part=snippet,contentDetails,status; maxResults=50) | always | 50 videos |
| `videos.list` (part=contentDetails,status) | only for **unmatched, available** videos (duration, region blocks, "gone since listed") | 50 unmatched videos |

Formula: `1 + ceil(N/50) + ceil(U/50)`. Worst case (nothing matches) is about 2 units per 50 items plus 1. The playlist and its looked-up details are cached for 10 minutes (`IMemoryCache`), so preview → import, or a repeat preview, costs **0 units**.

Measured through the running app (Staging, database `MintPlayer_S10`):

| Playlist | Videos | Units (`playlists`/`playlistItems`/`videos`) | In catalog by id | Matched | False + |
|---|---|---|---|---|---|
| `PL1QxplF7jh-264_6EZo_L5LXAl-L1nEBd` "90s,2000s,Today's Hits" — **the ~100-video run** | 98 | **5** (1/2/2) | 3 | 3 | 0 |
| `PLPhVoQzmOBxMm4SiW97U89C6Dsz1VRQim` "80s and 90s Hits Playlist" | 100 | 5 (1/2/2) | 2 | 2 | 0 |
| `PLGBuKfnErZlDXoq9Naa6qYtstCv_8CVRQ` "Late 90s Early 2000s Hits Clean" | 100 | 5 (1/2/2) | 1 | 1 | 0 |
| `PLNAzQwj53YOzFdBtMPEI9rmO_vLAy7IsB` "80s 90s 2000s Mix" | 132 | 7 (1/3/3) | 0 | 0 | 0 |
| `PLcb1ZAWRDY7XtwMJ-Dw82WC0VgTDsRtBH` "Top 1000 - Best Hits ever!" | 1,131 | 46 (1/23/22) | 8 | 8 | 0 |
| `PLAQ7nLSEnhWTEihjeM1I-ToPDJEKfZHZu` "Happy songs / Upbeat songs…" | 1,632 | 66 (1/33/32) | 7 | 7 | 0 |
| `PL1Z-OXEakI6z-dEG3vYV9asg2gnfKWWhQ` "1980-2025 Greatest Hits" | 1,888 | 77 (1/38/38) | 1 | 1 | 0 |
| **Total** | **5,081** | **211** | **22** | **22 (100 %)** | **0** |

The whole spike, including discovery (one `search.list` at 100 units to find candidate playlists, 111 units for a brute-force overlap survey, and a 58-unit debug run), used **about 550 of the 10,000 daily units**.

## Matching method and verification

- **Canonical id.** One regex, `YouTubeUrl.VideoIdPattern`: an 11-character id after `watch?…v=`, `youtu.be/`, `/embed/`, `/shorts/`, `/live/`, `/v/` or `/e/`, on `youtube.com` (any subdomain: www, m, music), `youtube-nocookie.com` or `youtu.be`. The host must not follow a letter, which rejects `notyoutube.com`. Extra parameters and fragments are ignored. The **same constant** is evaluated inside the RavenDB index map (`Regex.Match(m.Value, YouTubeUrl.VideoIdPattern)`), so the index and the importer cannot drift apart.
- **Lookup.** `where YouTubeIds in (…)` over `Songs/ByYouTubeId`, 256 ids per query (a 1,888-video playlist takes 8 queries). Soft-deleted songs are not indexed. The index stores terms lowercased (the Lucene default analyzer), so the lookup is case-insensitive, while YouTube ids are case-sensitive. The importer therefore **re-checks every candidate in C#** with the case-sensitive matcher. Unit test: `Video_ids_are_case_sensitive`.
- **Ground truth, playlists.** A separate script tested each playlist video id by brute force as a bounded substring of *every raw media string* in the catalog, without using the app's regex. The endpoint found every such video (22 / 22) and reported nothing extra.
- **Ground truth, catalog.** A second script extracted ids from all 164 YouTube media URLs with WHATWG `URL` parsing (again not the app's regex), then queried the index for each one. **164 / 164** came back with the owning song. No video is shared by two songs, and every one of the 141 live songs has at least one YouTube medium.
- **Behaviour.** Playlist order and duplicates are kept. When several songs share a video, the lowest song id wins and the others are listed in `otherSongIds`. Draft songs match on the next import, so repeated imports are idempotent. After the drafts were soft-deleted, the videos were reported as unmatched again, as expected.

### Catalog overlap (reported honestly)

The production catalog is small and personal: 141 songs, many of them Belgian or Dutch. Even the best-overlapping public hit playlists share only 0–3 % of their videos **by id** with it. That caps real-world match rates for public playlists. The main use case is users importing their *own* playlists, and there the overlap depends on the user.

The misses are also "same song, different upload". In the 98-video run, *Uncle Kracker – Follow Me* is in the catalog under a different video id. Matching by id cannot find this, by design. A later "possible matches by title/artist" hint is an option, but it was not built.

## URL variants found in the catalog (164 YouTube media on 141 songs)

| Shape | Count |
|---|---|
| `https://www.youtube.com/watch?v={id}` | 129 |
| `https://m.youtube.com/watch?v={id}` | 27 |
| `https://youtu.be/{id}` | 6 |
| `https://www.youtube.com/watch?v={id}&ab_channel=…` | 2 |

There are no `/embed/`, `/shorts/`, `music.`, `http:` or whitespace variants today, but the parser and tests cover them (the unit tests exercise 25 video-URL forms and 12 playlist-URL forms). The non-YouTube media are Wikipedia (82), Vimeo (4), Dailymotion (3) and SoundCloud (1), and all of them are ignored correctly.

YouTube medium types in use: `MediumTypes/1` "Official Music Video" (117), `/13` "Official Audio" (~32), `/3` "Live Performance" (8), `/14` "Lyrics Video" (3), and one each of `/8`, `/10`, `/19`, `/20`.

## Draft-song proposal (implemented minimally)

- `createDraftsForUnmatched: true` creates one `Song` per distinct **available** unmatched video. Deleted and private videos are skipped. The song has `Title` = video title (raw), `Media = [{Value: "https://www.youtube.com/watch?v={id}", TypeId: <MediumType named "Official Music Video"> (MediumTypes/1)}]` and `TagIds = ["Tags/youtube-import-draft"]`. That tag is a fixed-id `Tag` "Draft (imported from YouTube)", created on first use. The drafts are added to the playlist at their YouTube positions.
- **Why a tag and not an `IsDraft` flag:** no domain or model change, it is visible and filterable in the existing admin UI, and "promoting" a draft just means removing the tag.
- **Who may create drafts:** only the **Editor or Administrator** group, because the catalog is curated. Anyone else gets 403 (the playlist import itself is open to any signed-in user). A plain user's import holds just the matched songs; the preview lists the rest.
- Measured: 98-video playlist → 3 matched + 93 drafts = 96 tracks, 2 skipped (1 private, 1 deleted). A re-preview then showed 96 / 98 matched.
- **Suggested follow-ups inside F16 (not built):** strip `(Official Music Video)` / `[4K Upgrade]` / `Artist - ` from draft titles, and try to link an `Artist` by the channel name (`… - Topic`, `…VEVO`); an editor review list under the draft tag; possibly title-based "did you mean" suggestions for unmatched videos.

## Risks and findings

| Risk | Observation / mitigation |
|---|---|
| **Quota: 10,000 units/day per project** | Each import costs about 1 + 2 per 50 videos, so a 100-video import is 5 units and a maximal 5,000-video playlist is about 201. That is roughly 2,000 imports of 100 videos per day. The 10-minute cache stops repeat charges. `quotaExceeded` maps to HTTP 429. For many users, add a per-user daily limit. Never use `search.list` in the product: it costs 100 units per call. |
| Deleted / private videos | They stay in playlists as "Deleted video" / "Private video" with no owner channel. They are flagged `available=false` from `status.privacyStatus` and the missing owner, and never get drafts. Seen: 2 / 98, 51 / 1,131, 45 / 1,632. Videos that `videos.list` doesn't return are also marked as gone. |
| Region blocks | `videos.list` → `regionRestriction.blocked` is surfaced as `blockedRegions`. It is common: 4 / 98, 135 / 1,131, 167 / 1,632. Nothing is filtered; it is informational, because a video blocked in one country still plays in Belgium. |
| **Quirk: `videos.list` 403 "forbidden" (location=`myRating`)** | One batch of 50 valid ids in the 1,632-video playlist failed this way, reproducibly, with an API key. Details are optional, so a failed batch is skipped and its videos are **not** marked gone. Quota and key errors still propagate. |
| Mixes (`RD…`), Liked (`LL`), Watch later (`WL`) | These cannot be read with an API key. Mixes get a clear 400. `LL`/`WL` fail parsing, and anything else private returns 404. |
| Playlist size | The API caps playlists at 5,000 videos. There is a paging cap `YouTube:MaxPages` (default 100 pages, i.e. 5,000 videos). The pages are fetched one after another (38 `playlistItems` calls plus 38 `videos` calls for 1,888 videos), so the UI needs a spinner for large lists. |
| Key leakage | The key is sent only as the `X-Goog-Api-Key` header. A unit test asserts it never appears in a URL. .NET also redacts query strings in HttpClient logs (`?*`), and a grep of the app log confirmed there was no `key=`. |
| Case-insensitive index terms | Handled by the case-sensitive C# re-check, described above. |
| Concurrency on the cache entry | The details dictionary is updated under a lock. At worst two concurrent previews of the same new playlist each pay the fetch once. |

## How it was measured (reproducible)

1. Created database `MintPlayer_S10` on http://localhost:8080 and seeded it with `RAVEN_DB=MintPlayer_S10 node scripts/seed-catalog.mjs`. `seed-catalog.mjs` now honours `RAVEN_DB` and `RAVEN_URL`. Result: 330 docs, 141 songs.
2. Ran the app with `ASPNETCORE_ENVIRONMENT=Staging`, `ASPNETCORE_URLS=http://localhost:5501`, `Spark__RavenDb__Database=MintPlayer_S10` and `YouTube__ApiKeyFile=<path>`. Registered a synthetic user (`s10-tester@example.com`) via `/spark/auth/register` + `/login?useCookies=true`, then granted it the `Editor` group claim directly in RavenDB for the drafts run.
3. Called the endpoints with curl or Node, then compared the results with the brute-force scripts. The scripts are scratch files and were not committed.

Left in `MintPlayer_S10`: two imported playlists owned by the test user, 93 soft-deleted draft songs, the draft tag and the test user. The database is disposable.
