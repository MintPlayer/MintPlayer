# Spike S8: Hetzner deploy (local part)

Date: 2026-09-27. Scope: PRD-Spark-Completion §6 S8, decisions D14, D21, D22, D23, D24. **Local only.** Nothing
was deployed, no SSH, no push, no secrets created. The VPS half (staging hostname, workflow run, restore of a
licensed backup) is still open. See "What needs the user" below.

## What is ready (in this commit)

| Artifact | Notes |
|---|---|
| `MintPlayer.Web/Dockerfile` | Runtime stage: **Node 22** (only the `node` binary, copied from `node:22-bookworm-slim`), `USER app` (1654), `EXPOSE 8080` only, a DataProtection key dir owned by `app`. Build stage runs `npm ci` from the lockfile **before** `dotnet publish` (see findings 1 and 2). |
| `MintPlayer.Web/MintPlayer.Web.csproj` | Absolute `NpmInstallWorkingDirectory` / `NodeModulesCheckPath` (finding 1). |
| `/health`, `/health/ready` (Program.cs) | `/health` = process is up (compose healthcheck). `/health/ready` = `GetStatisticsOperation` against the app database with a 5 s timeout, then 200 or 503 (deploy gate). |
| `MintPlayer.Web/BackupConfigurator.cs` | D23 startup configurator, same shape as `RevisionsConfigurator`. Section `RavenBackup`: `FolderPath` (empty means disabled, which is the dev default), full `0 2 * * *`, incremental `15 * * * *`, `RetentionDays` 30, optional `S3` / `Ftp` bound onto Raven's own settings types. Idempotent: it looks the task up by name and rewrites it only when the schedule, folder, retention or destination target changed. |
| `deploy/docker-compose.yml` | Pull-only prod stack, one-for-one with CodeCoverage: `mintplayer-raven` (pinned `7.1.10-ubuntu.22.04-x64`, internal network only), `mintplayer-raven-license` one-shot, `mintplayer-smtp` (`boky/postfix:v4.3.0`, `ALLOWED_SENDER_DOMAINS=mintplayer.com`, HELO `coverage.mintplayer.com`, IPv4 only, DKIM mount, queue volume), `mintplayer-app` (`ghcr.io/mintplayer/mintplayer:master`, Traefik `Host(mintplayer.com) \|\| Host(www.mintplayer.com)` + a permanent www to apex `redirectregex` middleware, networks `web` + `mintplayer-internal`). New compared with coverage: a `mintplayer-raven-backup-init` one-shot (chowns the backup volume), plus `raven-backups` and `dataprotection-keys` volumes. |
| `deploy/docker-compose.local.override.yml` | Local proof: no Traefik and no `web` network, app on `127.0.0.1:18081`, Raven not published (the native Raven owns 8080), licence and smtp behind `--profile full`. |
| `deploy/.env.example` | Variable names only. |
| `.github/workflows/deploy.yml` | GHCR build-push + attestation + best-effort public visibility, then an `appleboy/ssh-action` deploy to `/var/www/mintplayer`. It refetches `deploy/docker-compose.yml` at the pushed SHA, runs pull/down/up, and polls `/health/ready` 30 x 10 s. |

## Local proof: results

- **Image build:** OK on the third attempt, after findings 1 and 2 were fixed. The Angular production bundle is built inside the image.
- **Image size:** 539 MB unpacked and about 143 MB compressed (`docker save`). The aspnet:10.0 base is about 340 MB of that.
  - **Node's cost:** 125 MB unpacked (the binary alone) and 44 MB gzipped.
  - Using nodesource apt instead would add npm and apt metadata on top.
- **Stack:** `backup-init` exited 0, then Raven went healthy, then the app started. `/health` returned 200 and `/health/ready` returned 200.
- **SPA:** `/` returned 200 `text/html` with `<app-root>` and a hashed `main-*.js`. The deep link `/artist/1` returned 200 through the SPA fallback. This was in Production mode (static dist).
- **Node in the runtime:** `node --version` is v22.23.3, running as uid 1654.
- **Backup task:** the app logged `task mintplayer-periodic-backup created (id 6) -> /var/lib/ravendb/backups, full '0 2 * * *', incremental '15 * * * *', retention 30d`.
  - After a restart and a force-recreate it logged `is up to date`, so the configurator is idempotent.
  - The database record shows `MinimumBackupAgeToKeep 30.00:00:00`.
- **Backup files:** I seeded 200 docs and triggered the task by hand.
  - Full backup: `…ravendb-MintPlayer-A-backup/…ravendb-full-backup` (4.3 KB).
  - I added a doc and triggered an incremental: `…ravendb-incremental-backup` (2.1 KB).
  - Both files were written by `ravendb:ravendb`, which confirms the volume chown.
- **Restore:** see finding 3. `RestoreBackupOperation` is **refused on an unlicensed server** (`LicenseLimitException: Your license doesn't support adding periodic backups`), because the restored database record carries the backup task.
  - Instead I imported the task's own `.ravendb-full-backup` file into a scratch DB `MintPlayerRestoreTest` with smuggler, leaving out DatabaseRecord.
  - Source and restore counts matched: `CountOfDocuments` 200 = 200, collections `{"S8Probes":200}` on both sides, and the probe doc was present. I then hard-deleted the scratch DB.
- **DataProtection:** the key survived a `--force-recreate` (same key id), so the finding-4 fix works.
- **Teardown:** `down -v` was run, and no `mintplayer-s8*` containers or volumes remain. The local `ghcr.io/mintplayer/mintplayer:master` tag was removed. `mintplayer-s8:local` is kept for reuse.

## Findings (fixed in this commit unless noted)

1. **`dotnet publish` failed in a clean tree.** NodeServices 10.4.0 passes the *relative* `$(SpaRoot)` to its own `npm-install.proj`, which lives in the NuGet cache, so the path resolves there. The error was `The working directory "ClientApp/" does not exist`. Fixed with absolute paths in the csproj. The proper fix is upstream in NodeServices, which should pass `$(MSBuildProjectDirectory)`. That belongs to the batched Spark/SpaServices framework change.
2. **The SPA build broke on a fresh tree.** MintPlayer.Spark.Authorization's build target runs `npm i @mintplayer/ng-spark-auth` whenever that package is missing from node_modules. With no node_modules, this resolves the newest versions and rewrites package.json and the lockfile. The Angular build then failed with `@mintplayer/web-components/accordion` unresolved and `BsAccordionTabHeaderComponent` / `SparkAuthRouteConfig` missing. Fixed with `npm ci` before publish, which makes both npm steps no-ops. CI and any fresh clone would have hit this.
   - **Upstream:** the target should never run an unpinned `npm i` inside a publish.
   - **Separately:** the lockfile pins ng-bootstrap 22.4.0 / web-components 2.0.1 / ng-spark-auth 22.0.1, while npm now has 22.19.0 / 2.16.0 / 22.13.0. That is fine now that `npm ci` is used, but an upgrade should be deliberate.
3. **The restore test needs the licence.** An unlicensed 7.1.10 (`/license/status` `Type: None`, `HasPeriodicBackup: false`) accepts the *creation* of the periodic task and runs manual backups. It refuses a *restore* of a database whose record contains that task.
   - The PRD's restore test must therefore be repeated on the VPS, with a real `RestoreBackupOperation`, after licence activation.
   - An unlicensed prod probably cannot be relied on to keep running scheduled backups either. I have not verified that.
4. **DataProtection keys were ephemeral** (`/home/app/.aspnet/DataProtection-Keys`, "may not be persisted"). Every deploy would have signed everyone out and voided reset and confirmation links. Fixed with the `dataprotection-keys` volume and a pre-owned directory. XmlKeyManager still warns that keys are stored unencrypted at rest. That is acceptable on a single-tenant VPS; see the open questions.
5. **The backup volume is created root-owned.** `/var/lib/ravendb/backups` does not exist in the Raven image, which runs as uid 999. Fixed with the `mintplayer-raven-backup-init` one-shot.

## Off-box backup: which destinations the licence allows

- **RavenDB's own statement:** *"local disk drive backups are available in all versions, while cloud and remote backups are available with Professional and Enterprise licenses"* ([features: periodic backups](https://ravendb.net/features/administration/periodic-backups)). So under **Community** (the tier CodeCoverage's README implies for the VPS licence): local only, and S3, FTP/SFTP, Azure, GCS and Glacier are **not** allowed. *Uncertain until checked against the real licence.*
- **Recommendation (D23 fallback):** keep the task local-only, and ship off-box from the host. A cron job `rsync`s or `rclone`s `/var/lib/docker/volumes/mintplayer_raven-backups/_data` to a Hetzner Storage Box over SSH (port 23) and verifies checksums at both ends.
- **If the licence turns out to be Professional or higher:** set `RAVEN_BACKUP_S3_*` (Hetzner Object Storage is S3-compatible: `CustomServerUrl` + `ForcePathStyle`). The configurator adds it to the same task. FTP settings are bindable via `RavenBackup__Ftp__*`.
- **Retention:** the 30-day `RetentionPolicy` applies to every destination of the task. A host-side copy needs its own pruning.
- **To verify on the VPS, printing only the flags and never the licence:** `docker compose exec mintplayer-raven curl -s localhost:8080/license/status | grep -o '"\(Type\|HasPeriodicBackup\|HasCloudBackups\)":[^,]*'`

## What needs the user (VPS / DNS / GitHub)

1. **VPS directory:** `mkdir -p /var/www/mintplayer`, then create `.env` from `deploy/.env.example` with LF line endings.
2. **`raven-license.json`:** place it in `/var/www/mintplayer/`. It must exist *before* the first `up`, or Docker creates a directory at that path. Then check that `mintplayer-raven-license` exited 0 and that the licence flags are as above.
3. **DKIM:**
   - Generate a new key for `mintplayer.com` with selector `mail` and install it flat as `mail-dkim/mintplayer.com.private`, owned `101:104`.
   - Publish the `mail._domainkey.mintplayer.com` TXT record and verify with `opendkim-testkey`.
   - The deploy refuses to run without `mail-dkim/`.
4. **SPF:** add the VPS IPv4 to the `mintplayer.com` SPF record *before the first send* (DMARC `sp=reject`). Also confirm Hetzner has unblocked outbound port 25, or set `MAIL_RELAY_*`.
5. **GHCR:** after the first workflow run, check that the `mintplayer` package is **public**, or `docker login` on the VPS with a `read:packages` PAT.
6. **Repo secrets:** `VPS_HOST`, `VPS_USERNAME`, `VPS_SSH_KEY` (dedicated ed25519 key), and optionally `VPS_PORT` / `VPS_SSH_KEY_PASSPHRASE`.
   - ⚠️ The workflow deploys on pushes to **master**, which still holds the legacy app. Add the secrets (or merge the workflow) only when cutover is intended.
   - For the S8 staging acceptance ("a staging hostname serves the workflow's image"), either temporarily point the Traefik rule at a staging host, or run the workflow from the migration branch with the deploy `if:` relaxed. That is the user's call.
7. **DNS cutover (D22):** lower the TTL of the `mintplayer.com` / `www` A records to 300 s a day before, then repoint them to the VPS. Let's Encrypt issues on first request once DNS resolves there. Rollback = repoint.
8. **Restore test with the licence (finding 3):** run a real `RestoreBackupOperation` of a task backup into a scratch DB on the VPS, compare `CountOfDocuments` and `CountOfAttachments`, then hard-delete it.
9. **Off-box copy:** pick the target, then set up the host cron or the S3 settings, depending on the licence.

## Open questions

- **Server vs. client version:** the server is pinned to **7.1.10** (the same build as coverage, already cached on the VPS, and its licence behaviour is measured). The app's `RavenDB.Client` is **7.2.1**, and the dev compose uses `7.2-latest`. The local run worked (DB creation, indexes, backups, stats). Should prod move to a pinned `7.2.x` so server ≥ client? That would require re-measuring the licence one-shot.
- **SMTP hop:** `Smtp__UseStartTls=false` maps to MailKit `SecureSocketOptions.Auto`. If Postfix advertises STARTTLS with a self-signed cert, the send may fail certificate validation. This was not tested, because smtp was omitted locally. F9/D24 replaces this sender anyway, and should use `SecureSocketOptions.None` for the internal hop.
- **www redirect:** the `redirectregex` middleware and the dual-host TLS router were not exercised, since there is no Traefik locally. Verify with `curl -I https://www.mintplayer.com/x`, expecting a 308 to the apex.
- **DataProtection encryption at rest:** is it enough, or should the key ring go into RavenDB (it would then be covered by the backups) with a certificate? Right now a lost `dataprotection-keys` volume only means one global sign-out.
- **Startup budget:** the app healthcheck `start_period` is 300 s. If the D7 cutover import runs at startup, re-measure it on the VPS, like coverage's re-key.
- **SSR:** S8's "one SSR'd page works in the container" depends on the parallel SSR work. The runtime image already carries Node 22, so rebuild and re-run the local override once that work lands.
