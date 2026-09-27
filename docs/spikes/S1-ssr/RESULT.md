# Spike S1 — SSR/prerender with MintPlayer.AspNetCore.SpaServices.Prerendering

**Verdict: works.** `/song/{id}` is server-rendered by Node from the production build, gets its
data from RavenDB in `OnSupplyData` (no HTTP hop), returns title / OG tags / JSON-LD without JS,
and hydrates in the browser with no console errors (the server's DOM nodes are kept, not replaced).
Warm TTFB is about 25–30 ms p50. The static-prerender fallback (PRD risk table) is not needed.

Branch `spike/s1-ssr` (from `feature/spark-migration`). Test DB: `MintPlayer_S1` (141 songs seeded).

## What was built

| Piece | File |
|---|---|
| Server boot module (default export = JavaScriptServices renderer, `renderApplication`) | `ClientApp/src/main.server.ts` |
| Dependency-free `createServerRenderer` (replaces the archived `aspnet-prerendering` npm package, which uses `domain` and `url.parse`) | `ClientApp/src/app/ssr/server-renderer.ts` |
| `PRERENDER_DATA` token (`params.data` from ASP.NET; `null` in the browser) | `ClientApp/src/app/ssr/prerender-data.ts` |
| `app.config.server.ts` (`provideServerRendering()`), `provideClientHydration(withEventReplay())` in `app.config.ts` | |
| Public song page, outside the admin Shell: title, artists, `<time datetime>` release date, `[seo]` OG tags, `[canonicalUrl]`, `og:type=music.song`, MusicRecording JSON-LD | `ClientApp/src/app/pages/song/song-page.ts` |
| Data: prerender data → TransferState → (client-side navigation only) `GET /api/public/song/{id}` | `ClientApp/src/app/pages/song/public-song.service.ts` |
| `PublicSongReader`: song + `Include` of the artists in one RavenDB round-trip, soft-delete aware, id whitelist | `MintPlayer.Web/PublicSite/PublicSongReader.cs` |
| `ISpaPrerenderingService`: routes `song/{id}`; 404 when missing (still rendered); `SkipPrerendering()` for all non-public routes | `MintPlayer.Web/PublicSite/MintPlayerSpaPrerenderingService.cs` |
| `GET /api/public/song/{id}` | `MintPlayer.Web/Controllers/PublicSongController.cs` |
| `UseSpaPrerendering`, `DefaultPage=/index.csr.html`, `/amp/song/{id}` → 301 `/song/{id}` (D11) | `MintPlayer.Web/Program.cs` |
| Build: production config gets `"server": "src/main.server.ts", "ssr": true, "prerender": false` | `ClientApp/angular.json` |
| SW: `index` → `/index.csr.html`; `navigationUrls` exclude `/song/**` | `ClientApp/ngsw-config.json` |
| Seeder DB override: `RAVEN_DB=MintPlayer_S1 node scripts/seed-catalog.mjs` | `scripts/seed-catalog.mjs` |

Packages: `MintPlayer.AspNetCore.SpaServices` 10.5.0 → **11.0.0-rc.2**, and a new
`MintPlayer.AspNetCore.SpaServices.Routing` 11.0.0-rc.2 (which brings Prerendering in). We need 10.8+
because it keeps the response status: the 404 set in `OnSupplyData` still renders the page. That
version is only on the 11.0 rc line. npm: `@angular/platform-server@22.0.0` and `@angular/ssr@22.0.0`,
pinned to the same patch as `@angular/core`/`@angular/build`.

### How the build works (differs from the demo)
The demo uses the old webpack `@angular-devkit/build-angular:server` builder. We use the esbuild
`@angular/build:application` builder instead, with `server` + `ssr: true` + `prerender: false`:

- the output is ESM, `dist/ClientApp/server/main.server.mjs`, and our `default` export survives. The library's `prerenderer.js` `import()`s it and needs `isServerRenderer = true` on the function;
- the whole server bundle is self-contained (2.2 MB). **`node_modules` is not needed at runtime**. So keep `BuildServerSideRenderer=false`, because `true` makes publish copy all of `node_modules` in. The normal `npm run build -- --configuration production` then builds both the browser and the server bundle;
- with `ssr` enabled, the builder always writes the browser shell as `index.csr.html`, never `index.html`. So `spa.Options.DefaultPage = "/index.csr.html"` is required (production only; `ng serve` still serves `index.html`);
- `ssr: true` pulls in `@angular/ssr` as a build-time dependency. It is not used at runtime.

## Exit criteria

| Criterion | Result |
|---|---|
| `curl /song/{id}` returns the title, OG tags and JSON-LD without JS | **Met.** `curl http://localhost:5101/song/313` → 200 with `<title>The Chain \| MintPlayer</title>`, `og:title`, `og:description`, `og:url`, `og:type`, one `meta name=description`, canonical, `<h1>The Chain</h1>`, `<time datetime="1977-02-04">February 4, 1977</time>`, and `<script type="application/ld+json">{"@type":"MusicRecording",…,"datePublished":"1977-02-04","byArtist":[{"@type":"MusicGroup","name":"Fleetwood Mac",…}]}` |
| No hydration errors | **Met.** Checked with the Playwright MCP in a fresh context: 0 console errors/warnings; the `<h1>` and `<time>` nodes captured at `readyState=interactive` are the *same* nodes after bootstrap (hydrated, not re-rendered); 0 `/api/` requests (data came through TransferState); 1 MusicRecording block, 1 canonical, 1 description |
| Works in a production build | **Met** for a `dotnet publish -c Release` output run with `ASPNETCORE_ENVIRONMENT=Production` and `node` on PATH. *Not yet run in the Docker image*; that is S8 (requirements below) |
| `/amp/song/313` | 301 → `Location: /song/313` |
| Missing song `/song/999999` | 404 with the rendered "Song not found" page |
| Admin routes (`/home`, `/query/…`) | Prerendering skipped: plain CSR shell (no `ng-state`), 200 |

### Timings (Windows 11 dev box, Node 24.15, RavenDB local, sequential curl)
| Request | TTFB |
|---|---|
| First SSR request after start (Node spawn + bundle import) | 0.74 s / 2.34 s / 1.81 s (three restarts) |
| SSR `/song/313`, warm, n=50 | min 14 ms · **p50 31 ms** · p90 42 ms · max 65 ms |
| SSR 20 different song ids, warm | min 14 ms · **p50 24 ms** · max 73 ms |
| CSR shell (prerender skipped), n=50 | p50 4.3 ms |
| JSON API `/api/public/song/313`, n=50 | p50 7.6 ms |
| Browser navigation `responseStart` | 41 ms |

So SSR adds about 20–25 ms per page on top of the CSR shell. The cold first hit costs 1–2 s. Use a
warm-up request or a health check that touches one SSR page before the container reports ready.
Reproduce with `docs/spikes/S1-ssr/timings.sh`. `harness.mjs` calls the server bundle directly,
without .NET: `node harness.mjs <ClientApp/dist/ClientApp>`.

## Runtime-image requirements (for D14 / S8 Dockerfile)
- **Node ≥ 22.22.3** (Angular 22 `engines`: `^22.22.3 || ^24.15.0 || >=26`) must be on `PATH` in the **runtime** stage (`mcr.microsoft.com/dotnet/aspnet:10.0`, plus NodeSource `setup_22.x`, `nodejs` only). No npm and no `node_modules` at runtime. The NodeServices entrypoint is embedded in the DLL and written to a temp file. It needs a writable `TMPDIR` (/tmp) for `USER app`.
- Publish output must contain `ClientApp/dist/ClientApp/server/**` (main.server.mjs + chunks, about 2.2 MB) and `ClientApp/dist/ClientApp/browser/**` (with `index.csr.html`). The current `DistFiles` glob `ClientApp\dist\**` already covers both. Paths are relative to the content root (`/app`), so `WORKDIR /app` stays as it is.
- `options.NodePath` defaults to `node`. Set it only if node is not on PATH.
- Build stage: unchanged (SDK + Node 22). **But see the NodeServices bug below**: the csproj workaround must stay in place or publish fails.
- Forwarded headers are already configured. `og:url`/canonical come from the request origin, so Traefik must send `X-Forwarded-Proto/Host`.
- Proposed runtime-stage addition:
  ```dockerfile
  FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
  RUN apt-get update && apt-get install -y --no-install-recommends curl ca-certificates gnupg \
   && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
   && apt-get install -y --no-install-recommends nodejs \
   && apt-get purge -y gnupg && rm -rf /var/lib/apt/lists/*
  ```

## Issues found (need fixing; library fixes go in the same unit of work)
1. **NodeServices 11.0.0-rc.2 publish bug.** `nodeservices.targets` passes the relative `NpmInstallWorkingDirectory=ClientApp\` to `buildTransitive/npm-install.proj` via `<MSBuild>`. There it resolves against the NuGet cache folder, so `dotnet publish` fails with *"working directory ClientApp\ does not exist"*. Workaround in `MintPlayer.Web.csproj`: absolute `NpmInstallWorkingDirectory`/`NodeModulesCheckPath`. Fix in the library: pass `$([System.IO.Path]::GetFullPath(...))`, or `$(MSBuildProjectDirectory)`-rooted paths.
2. **Leftover debug output in SpaServices 11.0.0-rc.2:** every request writes `Before the next middleware` / `After the next middleware` to stdout (178 lines in a short run). Remove before 11.0 GA.
3. **`@mintplayer/ng-seo` is not SSR-aware.**
   - `[jsonLd]` and `[canonicalUrl]` always append a fresh head element and never adopt the server-rendered one, so after hydration each existing block is doubled. Workaround in `main.ts`: remove the server copies before bootstrap. `<head>` is not hydrated, so this is safe.
   - `[seo]` uses `Meta.addTag` for `name=description`, so the page gets a second description next to `index.html`'s. Workaround in `SongPage`: remove the existing tag first.
   - Proper fix in ng-seo: adopt or replace existing elements (e.g. mark them with `data-ng-seo`), and use `updateTag`. `[seo]` also emits no `og:type`/`og:image`.
4. **`@mintplayer/ng-base-url` `provideBaseHref()`** (no-arg form, used in `app.config`) throws on the server: its factory reads `BOOT_FUNC_PARAMS` but never declares it as a dependency. Worked around by providing `provideBaseHref(params)` per request in `main.server.ts`.
5. **Angular 22 `allowedHosts`.** `renderApplication` rejects an absolute `url` unless the host is allow-listed (an SSRF guard). We pass the path (`params.url`), as the demo does. Consequence: **no relative `HttpClient` calls during a server render**. All public-page data must come through `OnSupplyData`, which is the intended design anyway.
6. **npm peer conflicts.** The lockfile was made without `--legacy-peer-deps` (there is no `.npmrc`). `npm install X --legacy-peer-deps` *prunes* peer packages from the lock (`@angular/animations`, lit, …). Use `npm install X --force`, which keeps them. A plain install fails ERESOLVE because `@angular/*@^22` resolves to 22.0.8 while the lock pins 22.0.0. Commit an `.npmrc`, or bump all `@angular/*` together.

## Risks / open points for F2
- **Service worker.** ngsw serves the cached CSR shell for navigations, so without the `!/song/**` exclusion repeat visitors never get SSR: no TransferState, and an API call on every visit. F2 must list *every* public SSR route in `navigationUrls` exclusions, or use `navigationRequestStrategy: "freshness"`. Right after a deploy, the *old* SW serves at least one more navigation.
- **Admin Shell is not SSR-safe** (auth-dependent). Public pages need their own public shell (F2), and non-public routes must keep calling `SkipPrerendering()`. Otherwise an anonymous server render mismatches on hydration.
- **Development:** prerendering is off in Development (no server bundle while `ng serve` runs). Opt in with `Prerendering:Enabled=true` after a prod build. An `AngularPrerendererBuilder` watch build next to `ng serve` was not tried. F2 should decide whether dev SSR is needed at all.
- Not-found pages: the server renders "not found", but the browser re-fetches (and gets a 404 from the API) because nothing is put into TransferState for a miss. Cosmetic; transfer a "miss" marker in F3.
- Legacy canonical URLs are `/song/{id}/{title-slug}` (the old prerender service redirected `/song/{id}` → slug). F3 must decide whether to keep slugs and add the 301s. S1 only does `/song/{id}`.
- `Content-Type: text/html` has no `charset` on prerendered responses. Harmless (the page has `<meta charset>`), but worth fixing in the library.
- Critical CSS is inlined by the builder ("beasties"), about 6 KB per page. Fine.
- `DatePipe` renders the release date in `en-US` on both sides. It is a `DateOnly`, so there is no timezone mismatch. Real instants need F17/D33's `<app-timestamp>` (not prototyped here).

## PRD corrections
- F2: the boot module is `main.server.ts` built by the **application builder** (`server` + `ssr` + `prerender:false`), not the demo's webpack `:server` builder. Output: `dist/ClientApp/server/main.server.mjs`. `BuildServerSideRenderer` stays `false`.
- F2/F4: add "ngsw `navigationUrls` must exclude SSR routes" and "`DefaultPage=/index.csr.html`".
- F3: depends on the ng-seo SSR fixes (issue 3). Put them into the framework-change batch alongside the ng-base-url fix (issue 4) and the SpaServices fixes (issues 1–2).
- D2/D14: SpaServices must be ≥ 10.8 (currently only 11.0.0-rc.2). Runtime image: Node ≥ 22.22.3 only, **no node_modules**.
- S1 row: "works in the Docker image" is really covered by S8. S1 proved it on a publish output.
