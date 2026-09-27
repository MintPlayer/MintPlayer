# Spike S9: Listen-together sync (F13, D31)

**Verdict: feasible.** The exit criteria are met. The one open item is the Traefik check, which needs the S8 staging host.

| Exit criterion (PRD §6 S9) | Result |
|---|---|
| Drift < 1 s over a 10-minute session | Guest vs. room: **max 0.124 s / avg 0.066 s over 572 s**, 0 seeks. Host + guest paired run: **max 0.047 s, inter-client max 0.041 s / avg 0.020 s over 317 s**. |
| …including an ad/buffer stall on one client | A 10 s stall on the guest was re-synced **within 0.5 s** of the stall ending (one seek, then drift 0.05 s). A spontaneous YouTube glitch hit both clients mid-video (`UNSTARTED` at 215 s, see Risks) and recovered in 0.3 s. |
| Reconnect after an app redeploy rejoins at the right position | The app process tree was killed and restarted (`dotnet run` rebuild, about 36 s down). Both clients **reconnected on their own without a page reload**. The room was reloaded from RavenDB, and drift after the rejoin was 0.06 s / 0.10 s with **no seek needed**. |
| YouTube + SoundCloud | Both work through `@mintplayer/video-player`. SoundCloud over 50 s: host max 0.041 s, guest max 0.093 s. |
| Through Traefik | **Not measured** because there is no Traefik locally. The required settings are in §5 and need checking on the S8 staging host. |

Environment: Windows 11, local RavenDB 7 (database `MintPlayer_S9`), app on `http://localhost:5301`. Two separate Playwright Chromium contexts, both headless and muted: the host was signed in as the dev admin, the guest was anonymous ("Bob"). Server and clients ran on the same machine, so RTT was about 1 ms. The clock-offset estimate is therefore close to 0 here, and real-network jitter was not exercised (see Risks). YouTube ad requests (`doubleclick.net`) fail with `ERR_ADDRESS_INVALID` in this environment, so no real ad played. The stall was simulated as described in §4.

## 1. Design

```
 host / guests (Angular)                       ASP.NET Core (singleton RoomService)          RavenDB
 ─────────────────────────                     ─────────────────────────────────────         ───────
 /room/:id  RoomPage                           /ws/rooms/{id}   (UseWebSockets, 20 s ping)
   RoomSyncService ── ws ──────────────────▶   LiveRoom (in memory)                          Rooms/{id}
     clock sync (ping/pong, min-RTT of 8)        connections, skip votes, SemaphoreSlim  ──▶  RoomDocument
     250 ms drift loop ──▶ PlayerService         RoomDocument (queue, current, anchor)        (persisted on
       (global <video-player> card)              broadcast full state on every change          every change)
 POST /api/rooms (signed-in) ─────────────▶   create (max 3 open rooms per host)
```

- **Anchor, not a ticking clock.** Playback state is `{mediaUrl, positionSec, serverTimeMs, playing}`: "at server time T the item was at P". Position at any moment = `P + (now − T)` while playing. Nothing is written while a track plays; only play/pause/seek/next/add/vote write. Because the anchor is persisted, a room reloaded after a restart resumes at the wall-clock-correct position, which is why the rejoin needed no seek.
- **Server is the authority, clients follow.** Every mutation runs under a per-room `SemaphoreSlim`, is persisted (`Rooms/{token}`, a plain RavenDB document rather than a Spark entity), and is then broadcast as the full state. The state is small (queue ≤ 200 items), so there is no diffing.
- **Live half in memory, durable half in RavenDB.** Connections and skip votes live only in memory. A room is dropped from memory when its last socket leaves and is reloaded on the next join.
- **Unguessable ids.** 128 random bits, base64url (`hZ8mF08iv3YLuOesossyPw`).
- **Access (D31).** Anyone with the link can listen (anonymous listeners pass `?name=` and are shown as "Name (guest)"). Only signed-in users can add or vote-skip. Only the host (the identity user who created the room, identified by the auth cookie on the upgrade request) can play/pause/seek/next/lock/close. The cap is 50 listeners; the host is always admitted. The host can lock the room to new listeners.
- **Cross-site WebSocket hijacking guard.** CORS does not apply to WebSockets, so the upgrade is refused (403) when `Origin` does not match the request host. Behind Traefik this relies on `UseForwardedHeaders` (already configured) to restore the public host.
- **Client (`RoomSyncService`)**:
  - **Clock sync.** Offset = `t1 − (t0+t2)/2`, keeping the lowest-RTT sample of the last 8. It pings 5× on (re)connect, then every 10 s.
  - **Drift loop (250 ms).** `local` = the player's last `currentTime` (YouTube reports every 50 ms) extrapolated by elapsed time. It compares `local` against `expected`:
    - **Hard seek** when |drift| > 0.75 s.
    - **Soft seek** when |drift| > 0.3 s persists for 8 s. This was added after SoundCloud sat at a steady −0.63 s start-up offset.
    - Seek target = expected + an adaptive seek lead, an EWMA of how late seeks land.
    - After a seek or load there is a 2.5 s settle window.
  - **State following.** If the room is playing and the player is paused, unstarted, or ended mid-track, the client re-commands play (1.5 s cooldown). On the player's `playing` edge (end of an ad or buffering) it re-checks immediately.
  - **Reconnect.** Exponential backoff from 1 s to 10 s. `welcome` replays the full state.
- **Player integration.** Three pieces were added to `PlayerService`: `setPlaying`, `seek`, `setMuted` and `livePosition`, plus a `PlayerHandle` that `PlayerCard` registers. `@mintplayer/video-player` 20.x has **no public seek**, so the handle reaches the provider adapter's `setProgress` (YouTube `seekTo`, SoundCloud `seekTo(ms)`) through `VideoPlayerComponent.player$.value.playerInfo` (a private field). This needs a framework follow-up; see §6.
- **End of track.** Any client may report `ended{itemId, durationSec}`. The server advances only if the item is still current and the room position agrees it is at the end (≥ duration − 5 s, and ≥ 2 s). This drops a stale `ended` when two consecutive items share a url, and it drops the spurious mid-video `ENDED` described under Risks. Both were found in this spike.

## 2. Protocol (JSON text frames, camelCase, `type`-discriminated)

Client → server:

| type | payload | who |
|---|---|---|
| `ping` | `t0` (client epoch ms) | anyone; answered outside the room lock |
| `play` / `pause` | — | host |
| `seek` | `positionSec` | host |
| `next` | — | host |
| `lock` | `locked` | host (closes/reopens the room to new listeners) |
| `close` | — | host (ends the room; sockets closed) |
| `add` | `url`, `title?` (http(s), ≤ 2048 chars, queue ≤ 200) | signed-in |
| `voteSkip` | — | signed-in; majority of connected distinct signed-in users |
| `ended` | `itemId`, `durationSec` | anyone; dedup by item id + position check |

Server → client:

| type | payload |
|---|---|
| `welcome` | `you{connectionId,isHost,signedIn,displayName}`, `state` |
| `state` | `state{roomId,name,hostName,hostOnline,locked,closed,queue[],currentItemId,anchor{mediaUrl,positionSec,serverTimeMs,playing},skipVotes,votesNeeded,listeners[]}` |
| `pong` | `t0`, `t1` (server epoch ms) |
| `error` | `code` (`denied`/`full`/`locked`/`not-found`), `message`, `fatal?` (then the socket is closed with 1008) |

Inbound frames are limited to 16 KiB (`ReadMessage(maxBytes)` of `MintPlayer.Dotnet.SocketExtensions` **10.0.1**, the published net10 package; the source in the Spark repo is `libs/socket_extensions`, currently 11.0.1-preview for net11). HTTP: `POST /api/rooms {name?}` returns `{id,name}` (401 for anonymous callers, 429 above 3 open rooms per host). `GET /api/rooms/{id}` returns a summary.

## 3. Measurements

| Run | Duration | Client | max \|drift\| | avg | p95 | seeks |
|---|---|---|---|---|---|---|
| Soak A (YouTube, Big Buck Bunny) | 572 s (≈ 9.5 min) | guest vs. room | 0.124 s | 0.066 s | 0.104 s | 0 |
| Soak B (YouTube, after fixes) | 317 s | host vs. room | 0.019 s | 0.004 s | 0.011 s | 0 |
| | | guest vs. room | 0.047 s | 0.024 s | 0.030 s | 0 |
| | | **host vs. guest** | **0.041 s** | **0.020 s** | 0.027 s | — |
| Pre-restart paired run (YouTube) | 78 s | host vs. guest | 0.056 s | 0.039 s | 0.050 s | — |
| SoundCloud (Forss, Flickermood) | 50 s | host / guest vs. room | 0.041 / 0.093 s | 0.034 / 0.088 s | — | 0 |

Soak A's host page was taken over mid-run by another agent sharing the Playwright MCP browser (its default page was navigated away), so only the guest series spans the full 9.5 min. Afterwards both clients ran in dedicated browser contexts. Soak B was cut to about 5 min for time. Samples are taken every 250 ms while in sync and outside the settle windows. Drift is measured against the room anchor using the client's own clock-offset estimate.

**Late join.** A client joining at room position 623 s seeked 0 → 624.9 s, then made two corrections (−1.1 s, +1.2 s) while its seek lead was learned. That was about 5 s to lock. This oscillation led to the EWMA (factor 0.5) on the lead.

**Stall (guest, 10 s).** The guest player was paused and its drift correction frozen for 10 s, which stands in for a long ad or buffering stall. At the end of the stall drift was −9.91 s. The loop re-played and seeked (lead 0.11 s), and 0.5 s later drift was +0.05 s. It stayed at 0.04–0.06 s for the next 20 s. The plain "user presses pause" path goes through the same re-play branch (`replays` counter), just without the freeze.

**Restart.** The process tree was killed at t = 0 and `dotnet run` restarted (including rebuild). Both sockets reconnected at t = +36.2 s, within 50 ms of each other; the delay is rebuild time plus the 10 s backoff cap. The server log shows `Room … loaded from RavenDB (anchor … playing=True)`. While the server was down, the clients kept playing and kept extrapolating the anchor locally, so there was nothing to correct on rejoin: drift was 0.10 s (host) and 0.06 s (guest) with 0 seeks. With the Angular dev server's live-reload active, the pages still did **not** reload.

**Permissions** (anonymous guest). `pause` returned "Only the host controls playback." `add` and `voteSkip` returned "Sign in to …". `next` returned "vote to skip instead". **Cap:** with 2 connected, 48 more sockets got `welcome` and the rest got `full`. An unknown room returned `not-found`. An upgrade with `Origin: https://evil.example` got **403**. An anonymous `POST /api/rooms` got **401**.

## 4. What the numbers do and don't cover

- Server and clients shared one machine (RTT ≈ 1 ms), so the anchor math, the drift loop, seek behaviour and restart recovery are proven. Clock-offset quality over a real WAN or mobile link is not. The design (min-RTT NTP-style sample, 10 s refresh) is standard and its error is bounded by RTT/2, which is well under the 0.75 s threshold on any sane connection.
- No real ads played (the ad hosts are unreachable here). The stall was simulated. A real mid-roll on one client shows up as the player leaving `playing` for the ad and returning, and that is exactly the re-play-then-seek path measured above.
- Both players were muted, which headless autoplay requires. The room page has a Mute button, and a real browser needs one user gesture to autoplay unmuted.

## 5. Traefik (production, D14)

Traefik proxies WebSocket upgrades on a normal HTTP router with **no extra labels**. The existing CodeCoverage-style labels (`Host(...)`, `websecure`, `letsencrypt`, `loadbalancer.server.port`) are sufficient. Check or configure the following on the S8 staging host:

1. **Entrypoint timeouts (Traefik v3).** `entryPoints.websecure.transport.respondingTimeouts.readTimeout` defaults to **60 s** in v3 (it was 0 in v2), and `idleTimeout` defaults to 180 s. A long-lived WS connection may be cut at those deadlines. Set `--entryPoints.websecure.transport.respondingTimeouts.readTimeout=0` (and `idleTimeout` ≥ 300s) on the Traefik static config, **or** accept periodic reconnects. The client reconnects within 1 s and resyncs without an audible glitch, because playback is local. **Verify on staging:** hold a room open for more than 5 min and count `reconnects` in `window.__room.stats()`.
2. **Keep-alive.** The server sends WebSocket pings every 20 s (`WebSocketOptions.KeepAliveInterval`). That keeps Docker/NAT/Hetzner idle timers from dropping idle sockets.
3. **Forwarded headers.** Already configured with `UseForwardedHeaders` (XFF/XFP/XFH). The Origin check depends on `X-Forwarded-Host` (or Traefik's passed-through `Host`) matching the public hostname. Traefik passes `Host` through by default.
4. **Single replica.** Room connections live in one process. Scaling to more than one app container needs either sticky routing by room id or a backplane (Redis pub/sub, or RavenDB changes-API fan-out). Keep `replicas: 1`, which matches the current compose.
5. **Redeploys.** A container swap drops every socket. The measured behaviour (reconnect with backoff, resume from the persisted anchor) covers this. Compose `stop_grace_period` can stay at its default.

## 6. Production-ready vs. prototype

**Keep as-is (production-shaped):** the anchor model and its persistence, the per-room lock, broadcasting full state, the permission matrix (D31), unguessable ids, the listener cap and host-room limit, the Origin guard, the inbound size limit, the ping/pong clock sync, the drift loop (hard/soft thresholds, adaptive seek lead, settle window, re-play on stall), the reconnect backoff, and the end-of-track validation.

**Prototype, to do in P5b (F13):**
- **Framework: a public `seek()` on `VideoPlayer` / `VideoPlayerComponent`** (`@mintplayer/video-player`, `@mintplayer/ng-video-player`), replacing the private `playerInfo.adapter.setProgress` reach-through in `PlayerCard`. Also useful: map YouTube `BUFFERING` to a state, so buffering can be told apart from playing.
- Room expiry after inactivity (D31). `LastActivityAt` is recorded but there is no sweep yet; this could be a RavenDB `@expires` on close/idle or a hosted service.
- Host hand-over (F13). This is not built. It would be a `handover{connectionId}` message that sets `HostUserId` to a signed-in listener.
- Room UX: a "Listen together" entry point from a playlist or the queue, share-link copy, display-name prompt (it is only `?name=` today), and a listener list.
- While in a room, the global player's own transport (sidebar next/previous, the card's close button, the YouTube iframe controls) is not locked. The drift loop overrides local play/pause, but a local "next" would fight the room.
- Start-of-item lead-in: every client starts about 1–1.5 s late and seeks once. Anchoring new items ~2 s in the future would remove that first seek.
- Rate limiting per connection (the message flood guard is currently only the 16 KiB frame cap), and structured drift telemetry in place of the `window.__room` debug hook.
- Tests: a `RoomService` unit test for the anchor/permission/advance logic, and a `RoomSyncService` spec with a fake player.

## 7. Risks

- **YouTube emits odd states.** In one run both clients' YouTube players simultaneously went `UNSTARTED` at 215 s and restarted from 0. It was most likely a blocked mid-roll ad break. Later, one client reported a spurious `ENDED` mid-video, which the first implementation accepted, and that skipped the track for the whole room. This is fixed by validating `ended` against the room position (client and server) and by treating a mid-track `ended` like a pause. It is still the main risk: provider players are black boxes, so the server must keep treating client reports as hints and never as authority.
- **Private API reach-through for seek.** This breaks silently if `@mintplayer/video-player` renames `playerInfo`. It is mitigated by the framework follow-up above.
- **Autoplay policies.** Unmuted autoplay needs a user gesture per tab, so a guest joining by link must click once, for example on a "Join" button. That is fine for UX but must be designed in.
- **Single-process state.** See §5.4. This is fine for MintPlayer's scale, and a backplane is only needed if the app is ever scaled out.
- **Spotify / DRM providers** have not been tried. Their embeds may not allow programmatic seek or reliable position reports.

## Files

- Server: `MintPlayer.Web/Rooms/{RoomDocument,LiveRoom,RoomService,RoomEndpoints}.cs`, `Program.cs` (UseWebSockets, `/ws` excluded from the SPA proxy, `MapRooms`), `MintPlayer.Web.csproj` (`MintPlayer.Dotnet.SocketExtensions` 10.0.1).
- Client: `ClientApp/src/app/room/{room-protocol,room-sync.service,room-page}.ts`, routes `/room` and `/room/:id`, `player/player.service.ts` (setPlaying/seek/setMuted/livePosition/registerPlayer), `player/player-card.ts` (mute binding + seek handle).
- Dev note: the host's SpaServices appends its own free `--port` to `npm start` (`Option 'port' has been specified multiple times. The value '55019' will be used`), so changing the start script's port is not needed to avoid collisions between worktrees. The ASP.NET port (`ASPNETCORE_URLS`) is the only one that matters.
