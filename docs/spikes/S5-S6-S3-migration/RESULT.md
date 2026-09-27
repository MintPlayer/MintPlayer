# S5 / S6 / S3 — migration, lyrics timing, identity round-trip (2026-09-27)

Spikes S5, S6 and S3 of `docs/PRD-Spark-Completion.md` §6. Source: `MintPlayer_Snapshot_20260927`
(LocalDB, schema `mintplay`). Target: RavenDB 7.2 at `http://localhost:8080`, databases
`MintPlayer_Migration` (snapshot) and `MintPlayer_S3` (snapshot copy + 2 synthetic users). The app
references **MintPlayer.Spark 10.0.0-preview.41**. No personal values were printed; reports list ids and counts only.

| Spike | Exit criterion | Result |
|---|---|---|
| S5 | Reconciler green, 0 unresolved refs, live counts = public API | **Met**: GREEN, 1176 refs / 0 unresolved, 138 / 9 / 141 / 12. The "10 random subjects in the new UI" check was replaced by reconciler sample dumps (no Angular app was run) — **UI spot check still open** |
| S6 | 5 songs highlight within 0.05 s of legacy; mismatch rate | **Met**, all 20 timelines: 0 mismatches over 507 655 samples, max start delta 0 s |
| S3 | Password login, TOTP, recovery code, Administrator rights | **Met**, with one blocking finding: log-in **by email** fails for 750 of 752 users (§S3) |

## What was built

`MintPlayer.Migration` (.NET 10 console, in `MintPlayer.slnx`) and `MintPlayer.Migration.Tests` (xUnit, 51 tests, all passing).

```
Sources/   ILegacySource (IAsyncEnumerable per table + Capabilities), SqlSnapshotSource (Dapper, schema option)
Model/     LegacyRecords — neutral records of the *snapshot* schema (not the repo's latest ModelSnapshot)
Transform/ LegacyTransformer (pure: snapshot → MigrationPlan), Preflight, LyricsTransform, Playability,
           Primitives (Ids, SoftDelete, LegacyTime, Colors, RecoveryCodes)
Target/    SparkConventions (the DocumentStore AddSpark builds, incl. Spark's own internal
           ColorNewtonsoftJsonConverter via reflection), RavenWriter (recreate DB, BulkInsert with explicit ids,
           emails/ compare-exchange in cluster-wide batches, indexes from the MintPlayer.Web assembly, wait non-stale),
           JsonlWriter (--dry-run), BlogPost (stopgap type, see deviations)
Verify/    Reconciler, LyricsTimingCheck (S6 as a permanent check)
```

Also: `MediumType.Visible` (default `true`) added to MintPlayer.Domain (D15/F10) and `MediumType.json` re-synchronized
with `--spark-synchronize-model`. The `ViewHiddenMedia` right was not built.

`docs/spikes/S5-S6-S3-migration/S3Probe` — spike helper (not in the solution): `seed` inserts the synthetic users into
a `*_S3_Source` copy (it refuses any other database); `probe` drives the running app.

### Commands

```powershell
dotnet build MintPlayer.slnx
$sql = 'Server=(localdb)\MSSQLLocalDB;Database=MintPlayer_Snapshot_20260927;Integrated Security=true;TrustServerCertificate=true;ApplicationIntent=ReadOnly'
$exp = 'artists=138,people=9,songs=141,visibleMediumTypes=12,tags=49,tagCategories=6,playlists=11,publicPlaylists=2,blogPosts=10'
MintPlayer.Migration\bin\Debug\net10.0\MintPlayer.Migration.exe --dry-run --sql $sql --out out\dry     # JSONL (contains personal data!)
MintPlayer.Migration\bin\Debug\net10.0\MintPlayer.Migration.exe --run --sql $sql --database MintPlayer_Migration --out out\run --expect $exp
MintPlayer.Migration\bin\Debug\net10.0\MintPlayer.Migration.exe --verify-only --sql $sql --database MintPlayer_Migration --out out\verify --expect $exp
dotnet test MintPlayer.Migration.Tests
```

Exit codes: 0 green, 1 reconciler red, 2 pre-flight failed, 64 bad arguments. `--run` **deletes and recreates** `--database`.
Outputs: `issues.txt` (transform findings), `reconcile.txt`, `samples.txt` (catalog collections only).

## S5 — end-to-end migration

**Timings** (`--run`, whole snapshot): 10.6 s total = read 0.4 s, pre-flight 0.03 s, transform 0.1 s,
**database delete + create 6.0 s**, BulkInsert 1156 docs + 752 reservations 0.6 s, indexes (3) non-stale 0.4 s,
reconcile incl. S6 replay 3.1 s. `--verify-only`: 7.3 s. A second `--run` produces byte-identical documents
(the transform is deterministic; see deviations).

**Reconciler** (source counts come from independent SQL that re-implements the soft-delete rule; target counts from the stored documents):

| check | source | target | | check | source | target |
|---|---|---|---|---|---|---|
| Artists all / live | 145 / 138 | 145 / 138 | | Media on subjects | 507 | 505 (2 empty skipped) |
| People all / live | 15 / 9 | 15 / 9 | | SubjectTag pairs | 132 | 132 |
| Songs all / live | 141 / 141 | 141 / 141 | | ArtistSong (uncredited) | 154 (9) | 154 (9) |
| MediumTypes all / live / visible | 15 / 13 / 12 | 15 / 13 / 12 | | ArtistPerson | 11 | 11 |
| Tags all / live | 51 / 49 | 51 / 49 | | PlaylistSong | 116 | 116 |
| TagCategories all / live | 8 / 6 | 8 / 6 | | Likes / dislikes | 156 / 0 | 156 / 0 |
| Playlists all / live / public | 16 / 11 / 2 | 16 / 11 / 2 | | Songs with lyrics text | 127 | 127 |
| BlogPosts all / live | 10 / 10 | 10 / 10 | | Songs with timeline | 20 | 20 |
| Users | 752 | 752 | | Logins / roles / group claims | 34 / 2 / 2 | 34 / 2 / 2 |
| UserLikes docs | 3 | 3 | | Password hashes / 2FA on | 722 / 2 | 722 / 2 |
| Documents plan vs DB | 1156 | 1156 | | Authenticator keys / code sets | 13 / 2 | 13 / 2 |
| | | | | `emails/` reservations | 752 | 752 |

SHA-256 round trip 1156/1156 equal · references 1176 checked, **0 unresolved**, 0 live→deleted · indexes 3, 0 stale, 0 errors.

**Transform findings** (issues.txt): 1 song with a bare `DateDelete` kept live (Songs/274); 4 rows deleted by a user
without `DateDelete` (Artists/4, People/3, MediumTypes/7, /18 → `DeletedAt` null); 8 `created-fallback`; 2 empty media;
3 categories lose alpha; 2 tags without category (41, 42); 14 empty lyric texts; 4 two-version songs (identical text);
Playlists/16 keeps 3 duplicate tracks; 11 authenticator keys without 2FA enabled (kept); **2 Born dates at 22:00**
(People/97, /98 — a local midnight stored as UTC; the migration takes the Europe/Amsterdam date, i.e. the next day).
Jobs (629) and LogEntries (0) dropped.

## S6 — lyrics timing

Legacy (`legacy/.../linify.pipe.ts`, `app.component.ts`): the timeline has one entry per line of
`text.split('\n').filter(l => l !== '')` — **strictly empty** lines are skipped, a whitespace-only line counts; stored ×20
as `int[]`; the active line is the last one with `time < now`.
New (`ClientApp/src/app/lyrics/song-lyrics.ts`, `SongLyricsController`): `StartTimes[i]` belongs to line `i` of
`text.split('\n')` — **all lines, blanks included** — `null` = unsynced; active = last `start <= now`; the timing is
picked by `MediumUrl` = the URL being played, and the player plays the first `findApis`-playable URL of `Song.Media`
(`PlaylistPlaybackService.firstPlayable`).

Transform: entry `k` → index of the k-th non-empty line, `÷ 20.0`; blank lines and lines past a partial sync → `null`;
surplus entries dropped (none in production); keyed to the first playable medium (regexes copied from the six
video-player plugins). Result for the five spike songs, replayed at 10 ms against the **stored** documents, plus the app's
`/api/song/lyrics`:

| song | legacy entries | lines (all) | samples | mismatches | max start Δ |
|---|---|---|---|---|---|
| Songs/23 | 19 | 23 | 23 415 | 0 | 0 s |
| Songs/43 | 92 | 101 | 35 570 | 0 | 0 s |
| Songs/129 (partial) | 12 | 77 | 4 210 | 0 | 0 s |
| Songs/238 | 81 | 91 | 29 240 | 0 | 0 s |
| Songs/291 (partial) | 1 | 63 | 115 700 | 0 | 0 s |

All 20 timelines: 507 655 samples, **mismatch rate 0 %**, every synced line starts at exactly the legacy instant. The only
semantic difference is `<` vs `<=` at the exact boundary instant (0 s of drift). Songs/291: legacy *displayed* its latest
lyrics row, which has no timeline, so legacy never showed karaoke for it; D12 migrates the older row's 1-entry timeline.
The check runs in every reconcile (`LyricsTimingCheck`) and fails the run on any mismatch or Δ > 0.05 s.

## S3 — identity round-trip

A throwaway copy (`MintPlayer_S3_Source`, restored from the same .bak into `C:\Users\piete\SqlData\S3`, **dropped
afterwards**) received 2 synthetic users: (a) password-only with a **legacy V3 HMAC-SHA256 / 10 000-iteration** hash
(the format of 183 production hashes), (b) password (**V3 HMAC-SHA512 / 100 000**, 539 production hashes) + TOTP
(`[AspNetUserStore]/AuthenticatorKey`, `TwoFactorEnabled=1`) + 10 recovery codes + **Administrator** role. Both have
`UserName ≠ Email`, like 750 of 752 production users. Migrated into `MintPlayer_S3` (reconciler green), MintPlayer.Web
run with `ASPNETCORE_ENVIRONMENT=Staging`, `ASPNETCORE_URLS=http://localhost:5201`, `Spark__RavenDb__Database=MintPlayer_S3`.

```
FINDING  (a) login with legacy email + password — 401 (Identity API /login resolves the 'email' field with FindByNameAsync)
PASS  (a) login with legacy user name + password  — 200   (the old SHA-256 hash is rehashed to V3/SHA-512 on login)
PASS  (a) non-admin cannot create a MediumType  — 403
PASS  (b) password alone is challenged for 2FA  — 401 RequiresTwoFactor
PASS  (b) password + current TOTP from the legacy authenticator key  — 200
PASS  (b) /spark/auth/me carries the Administrator role  — roles ["Administrator"]
PASS  (b) Administrator creates a MediumType via POST /spark/po/{type} (X-XSRF-TOKEN)  — 201
PASS  (b) a wrong TOTP is rejected  — 401
PASS  (b) a legacy recovery code redeems  — 200;  reuse → 401;  another code → 200
PASS  S6 /api/song/lyrics for Songs/23, 43, 129, 238, 291 — StartTimes length = line count
```

The stored `emails/…` compare-exchange value has the same shape `UserStore` writes (`{"Object": "MintPlayerUsers/…"}`).

## Deviations from §5.2

1. **Recovery codes are stored verbatim, not SHA-256-hashed**, because the app runs Spark **preview.41**, whose
   `UserStore.RedeemCodeAsync` compares raw strings (`HashRecoveryCode` only exists on Spark master). The tool detects
   this by reflection on the referenced `UserStore<>` (`--recovery-codes auto`, override `hashed|plain`), so it flips
   automatically when the app upgrades Spark. A test pins the preview.41 behaviour.
2. **Roles** → `Roles[]` **and** a `group` claim with the same name. `Roles[]` feeds Identity (role claims → Spark ACL,
   `/spark/auth/me`); the `group` claim is what the app itself checks (`SongLyricsController`, `DevDataSeeder`).
3. **Users keep `ConcurrencyStamp`** (the PRD listed it as lost): it makes the transform deterministic, so
   `--verify-only` and repeated runs reproduce every byte. Likewise rows without audit dates (MediumTypes and
   Playlists have none in production; 3 subjects with `DateInsert` 0001-01-01 and no `DateUpdate`) get the snapshot's
   **as-of instant** (its latest audit timestamp) instead of "now"; `--migrated-at` overrides.
4. **BlogPosts** use a stopgap `MintPlayer.Migration.Target.BlogPost : Entity` (Title, Headline, Body, AuthorId,
   Published = CreatedAt) in the `BlogPosts` collection — **replace it with the F1 domain entity**.
5. MediumType: legacy `Description` → `Name`, target `Description` null. Playlist: legacy `Description` → `Name`, target
   `Description` null. `PictureUrl` "" → null (all 752 are empty).
6. Dates with a time part in Born/Died/Released are read as UTC instants and converted to the Europe/Amsterdam date.
   DST: times inside the spring-forward gap move +1 h; ambiguous autumn times take the first (CEST) occurrence.
7. `--source api|composite` (PublicApiSource) is **not implemented** (`--source sql` only); the API cross-check of §5.3
   and the `SignInManager` smoke login inside the tool are not in the reconciler — S3 did the login check against the running app.
8. The round-trip check compares each planned entity with the entity re-loaded from RavenDB, both serialized through
   the same Newtonsoft serializer (with Spark's colour converter) and SHA-256-hashed over canonical JSON. It does not
   compare RavenDB's raw bytes.

## Open issues

1. **Blocking for cutover: log-in by email.** Legacy logs in with `FindByEmailAsync`
   (`AccountRepository`). Spark's `/spark/auth/login` is Microsoft's `MapIdentityApi`, which passes the `email` field to
   `PasswordSignInAsync(userName)` → `FindByNameAsync`. **750 of 752 production users have `UserName ≠ Email`**, so
   they could only log in by typing their legacy user name. Fix in the Spark PR (F6/F7): resolve the login by email
   (compare-exchange `emails/`) and fall back to the user name. Not fixed here; the migration keeps `UserName` verbatim.
   (Changing `UserName = Email` in the migration would also work, but drops the legacy user names.)
2. The UI spot check (10 random subjects in the new UI) was not done; `samples.txt` holds 10 transformed-vs-stored catalog dumps.
3. The karaoke timing is keyed only to the first playable medium. Legacy applied its single timeline to **whatever**
   recording played, so playing another medium (e.g. the second YouTube link) shows no karaoke after migration. This is
   intended by D29 (timing per medium); flag it if parity is wanted.
4. Of the 752 users, 183 still have SHA-256/10k hashes. They are rehashed on their first log-in, as S3 showed; nothing to do.
5. The database delete + create takes about 6 s of the 10 s run, because the delete waits for confirmation.

## PRD corrections

- §5.2 `AspNetUserTokens` row: recovery codes are **hashed only once Spark is past preview.41**; until then they are
  stored verbatim, and the tool picks the form automatically. Amend D8/F7 accordingly (F7's "already hashed —
  verify": true on master, **false in preview.41**).
- §5.2 `AspNetUserRoles` row: → `Roles[]` **plus** `group` claims (the app authorises on `group` claims).
- §5.2 "Lost by design": drop `ConcurrencyStamp` (it is kept).
- §5.2: MediumTypes and Playlists have **no audit columns** in production; Playlists soft delete is an `IsDeleted` bit,
  not `UserDeleteId`.
- §6.1: add "2 Born values carry 22:00 (UTC-shifted local midnight)"; the 20 timelines are 20 songs (§3.1 says 19).
- New finding for F6/Spark PR: **email log-in** (Open issue 1). S3's exit criterion is met only with the user name.
- §6 S6 wording: legacy skips only *empty* lines (`!== ''`), not whitespace-only "blank" lines.
