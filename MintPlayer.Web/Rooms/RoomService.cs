using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Raven.Client.Documents;

namespace MintPlayer.Web.Rooms;

/// <summary>
/// Listen-together rooms (F13 / spike S9): registry of live rooms + the socket protocol.
///
/// <para>Protocol (JSON text frames, camelCase, discriminated by <c>type</c>). Client → server:
/// <c>ping{t0}</c>, <c>play</c>, <c>pause</c>, <c>seek{positionSec}</c>, <c>next</c> (host only);
/// <c>add{url,title}</c>, <c>voteSkip</c> (signed-in only); <c>ended{itemId}</c> (anyone — dedup'd by item id);
/// <c>lock{locked}</c>, <c>close</c> (host only). Server → client: <c>welcome{you,state}</c>, <c>state{state}</c>,
/// <c>pong{t0,t1}</c>, <c>error{code,message}</c>.</para>
/// </summary>
public sealed class RoomService(IDocumentStore store, ILogger<RoomService> logger)
{
    public const int MaxListeners = 50;
    public const int MaxRoomsPerHost = 3;
    public const int MaxQueueLength = 200;
    public const int MaxInboundMessageBytes = 16 * 1024;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly ConcurrentDictionary<string, LiveRoom> rooms = new();
    private readonly SemaphoreSlim loadGate = new(1, 1);

    public static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>128 random bits, base64url — unguessable room token (D31).</summary>
    public static string NewToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string DocId(string token) => $"Rooms/{token}";

    // ---------------------------------------------------------------- create / load

    public async Task<RoomDocument?> CreateAsync(string hostUserId, string hostName, string? name, CancellationToken ct)
    {
        using var session = store.OpenAsyncSession();
        var open = await session.Query<RoomDocument>()
            .Where(r => r.HostUserId == hostUserId && !r.Closed)
            .CountAsync(ct);
        if (open >= MaxRoomsPerHost)
            return null;

        var now = DateTimeOffset.UtcNow;
        var doc = new RoomDocument
        {
            Id = DocId(NewToken()),
            Name = string.IsNullOrWhiteSpace(name) ? $"{hostName}'s room" : name.Trim()[..Math.Min(name.Trim().Length, 80)],
            HostUserId = hostUserId,
            HostName = hostName,
            CreatedAt = now,
            LastActivityAt = now,
            Anchor = new RoomAnchor { ServerTimeMs = NowMs() },
        };
        await session.StoreAsync(doc, doc.Id, ct);
        await session.SaveChangesAsync(ct);
        return doc;
    }

    /// <summary>The live room, loading it from RavenDB on first use (e.g. first join after an app restart).</summary>
    private async Task<LiveRoom?> GetOrLoadAsync(string token)
    {
        if (rooms.TryGetValue(token, out var live))
            return live;

        await loadGate.WaitAsync();
        try
        {
            if (rooms.TryGetValue(token, out live))
                return live;

            using var session = store.OpenAsyncSession();
            var doc = await session.LoadAsync<RoomDocument>(DocId(token));
            if (doc is null || doc.Closed)
                return null;

            live = new LiveRoom(doc);
            rooms[token] = live;
            logger.LogInformation("Room {Room} loaded from RavenDB (anchor {Pos:F1}s @ {Time}, playing={Playing})",
                token, doc.Anchor.PositionSec, doc.Anchor.ServerTimeMs, doc.Anchor.Playing);
            return live;
        }
        finally
        {
            loadGate.Release();
        }
    }

    public async Task<object?> GetSummaryAsync(string token)
    {
        var live = await GetOrLoadAsync(token);
        return live is null ? null : new { live.Doc.Name, live.Doc.HostName, listeners = live.Connections.Count, live.Doc.Locked };
    }

    // ---------------------------------------------------------------- socket

    public async Task HandleSocketAsync(HttpContext http, string token, string? userId, string displayName)
    {
        using var socket = await http.WebSockets.AcceptWebSocketAsync();
        var conn = new RoomConnection(Guid.NewGuid().ToString("N")[..12], socket, userId, displayName);

        var live = await GetOrLoadAsync(token);
        if (live is null)
        {
            await RejectAsync(conn, "not-found", "This room does not exist or was closed.");
            return;
        }

        await live.Gate.WaitAsync();
        try
        {
            var isHost = live.IsHost(conn);
            if (!isHost && live.Connections.Count >= MaxListeners)
            {
                await RejectAsync(conn, "full", $"This room is full ({MaxListeners} listeners).");
                return;
            }
            if (!isHost && live.Doc.Locked)
            {
                await RejectAsync(conn, "locked", "The host closed this room to new listeners.");
                return;
            }
            live.Connections[conn.Id] = conn;
            await conn.SendAsync(Serialize(new
            {
                type = "welcome",
                you = new { connectionId = conn.Id, isHost, signedIn = conn.SignedIn, displayName },
                state = Snapshot(live),
            }));
            await BroadcastStateAsync(live, except: conn.Id);
        }
        finally
        {
            live.Gate.Release();
        }

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var message = await socket.ReadMessage(MaxInboundMessageBytes); // throws on close / oversize
                await HandleMessageAsync(live, conn, message);
            }
        }
        catch (WebSocketException) { /* client went away */ }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Room {Room}: dropping connection {Conn} after a bad message", token, conn.Id);
        }
        finally
        {
            live.Connections.TryRemove(conn.Id, out _);
            await live.Gate.WaitAsync();
            try
            {
                if (live.Connections.IsEmpty)
                    rooms.TryRemove(token, out _); // durable state is in RavenDB; reload on next join
                else
                    await BroadcastStateAsync(live);
            }
            finally
            {
                live.Gate.Release();
            }
            if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                try { await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None); } catch { }
            }
        }
    }

    private static async Task RejectAsync(RoomConnection conn, string code, string message)
    {
        await conn.SendAsync(Serialize(new { type = "error", code, message, fatal = true }));
        await conn.Socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, code, CancellationToken.None);
    }

    private async Task HandleMessageAsync(LiveRoom live, RoomConnection conn, string message)
    {
        var msg = JsonNode.Parse(message) as JsonObject ?? throw new JsonException("Expected an object");
        var type = (string?)msg["type"];

        // Clock sync is answered immediately, outside the room gate, so queueing behind state changes
        // never inflates the measured round-trip.
        if (type == "ping")
        {
            await conn.SendAsync(Serialize(new { type = "pong", t0 = (double?)msg["t0"] ?? 0, t1 = NowMs() }));
            return;
        }

        await live.Gate.WaitAsync();
        try
        {
            var doc = live.Doc;
            var now = NowMs();
            var isHost = live.IsHost(conn);
            string? denied = null;
            var changed = false;

            switch (type)
            {
                case "play" or "pause":
                    if (!isHost) { denied = "Only the host controls playback."; break; }
                    if (doc.CurrentItemId is null) break;
                    Reanchor(doc, now, playing: type == "play", positionSec: null);
                    changed = true;
                    break;

                case "seek":
                    if (!isHost) { denied = "Only the host controls playback."; break; }
                    if (doc.CurrentItemId is null) break;
                    var pos = Math.Max(0, (double?)msg["positionSec"] ?? 0);
                    Reanchor(doc, now, playing: doc.Anchor.Playing, positionSec: pos);
                    changed = true;
                    break;

                case "next":
                    if (!isHost) { denied = "Only the host can skip; vote to skip instead."; break; }
                    Advance(live, now);
                    changed = true;
                    break;

                case "ended":
                    // Anyone may report the end; only the report for the CURRENT item advances (dedup).
                    // The room position must agree that the item is (nearly) over: this drops a stale `ended`
                    // from a client whose player still reports the previous item's end (same url twice in a
                    // row), and a spurious mid-video ENDED (seen from YouTube around a blocked ad break).
                    var roomPos = doc.Anchor.PositionAt(now);
                    var duration = (double?)msg["durationSec"] ?? 0;
                    if ((string?)msg["itemId"] is { } endedId && endedId == doc.CurrentItemId
                        && roomPos >= 2 && (duration <= 0 || roomPos >= duration - 5))
                    {
                        Advance(live, now);
                        changed = true;
                    }
                    break;

                case "add":
                    if (!conn.SignedIn) { denied = "Sign in to add songs."; break; }
                    var url = ((string?)msg["url"])?.Trim();
                    if (url is null || url.Length > 2048 || !Uri.TryCreate(url, UriKind.Absolute, out var uri)
                        || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
                    { denied = "Not a valid media url."; break; }
                    if (doc.Queue.Count >= MaxQueueLength) { denied = "The queue is full."; break; }
                    var title = ((string?)msg["title"])?.Trim();
                    var item = new RoomQueueItem
                    {
                        Id = Guid.NewGuid().ToString("N")[..10],
                        Url = url,
                        Title = string.IsNullOrEmpty(title) ? url : title[..Math.Min(title.Length, 200)],
                        AddedBy = conn.DisplayName,
                    };
                    doc.Queue.Add(item);
                    if (doc.CurrentItemId is null)
                        StartItem(live, item, now);
                    changed = true;
                    break;

                case "voteSkip":
                    if (!conn.SignedIn) { denied = "Sign in to vote."; break; }
                    if (doc.CurrentItemId is null) break;
                    live.SkipVotes.Add(conn.UserId!);
                    if (live.SkipVotes.Count >= live.VotesNeeded())
                        Advance(live, now);
                    changed = true;
                    break;

                case "lock":
                    if (!isHost) { denied = "Only the host can lock the room."; break; }
                    doc.Locked = (bool?)msg["locked"] ?? true;
                    changed = true;
                    break;

                case "close":
                    if (!isHost) { denied = "Only the host can close the room."; break; }
                    doc.Closed = true;
                    changed = true;
                    break;

                default:
                    denied = $"Unknown message type '{type}'.";
                    break;
            }

            if (denied is not null)
            {
                await conn.SendAsync(Serialize(new { type = "error", code = "denied", message = denied }));
                return;
            }
            if (!changed)
                return;

            doc.LastActivityAt = DateTimeOffset.UtcNow;
            await PersistAsync(doc);
            await BroadcastStateAsync(live);
            logger.LogInformation("Room {Room}: {Type} by {Who} -> item {Item} @ {Pos:F2}s playing={Playing}",
                doc.Id, type, conn.DisplayName, doc.CurrentItemId, doc.Anchor.PositionSec, doc.Anchor.Playing);

            if (doc.Closed)
            {
                foreach (var c in live.Connections.Values)
                    _ = c.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "closed", CancellationToken.None);
            }
        }
        finally
        {
            live.Gate.Release();
        }
    }

    // ---------------------------------------------------------------- state transitions

    private static void Reanchor(RoomDocument doc, long now, bool playing, double? positionSec)
    {
        var pos = positionSec ?? doc.Anchor.PositionAt(now);
        doc.Anchor = new RoomAnchor { MediaUrl = doc.Anchor.MediaUrl, PositionSec = pos, ServerTimeMs = now, Playing = playing };
    }

    private static void StartItem(LiveRoom live, RoomQueueItem? item, long now)
    {
        live.SkipVotes.Clear();
        live.Doc.CurrentItemId = item?.Id;
        live.Doc.Anchor = new RoomAnchor { MediaUrl = item?.Url, PositionSec = 0, ServerTimeMs = now, Playing = item is not null };
    }

    private static void Advance(LiveRoom live, long now)
    {
        var queue = live.Doc.Queue;
        var index = queue.FindIndex(i => i.Id == live.Doc.CurrentItemId);
        StartItem(live, index >= 0 && index + 1 < queue.Count ? queue[index + 1] : null, now);
    }

    private async Task PersistAsync(RoomDocument doc)
    {
        using var session = store.OpenAsyncSession();
        await session.StoreAsync(doc, doc.Id);
        await session.SaveChangesAsync();
    }

    // ---------------------------------------------------------------- snapshots

    private static object Snapshot(LiveRoom live) => new
    {
        roomId = live.Doc.Id["Rooms/".Length..],
        name = live.Doc.Name,
        hostName = live.Doc.HostName,
        hostOnline = live.Connections.Values.Any(live.IsHost),
        locked = live.Doc.Locked,
        closed = live.Doc.Closed,
        queue = live.Doc.Queue,
        currentItemId = live.Doc.CurrentItemId,
        anchor = live.Doc.Anchor,
        skipVotes = live.SkipVotes.Count,
        votesNeeded = live.VotesNeeded(),
        listeners = live.Connections.Values.Select(c => new { c.DisplayName, c.SignedIn, isHost = live.IsHost(c) }),
    };

    private static async Task BroadcastStateAsync(LiveRoom live, string? except = null)
    {
        var json = Serialize(new { type = "state", state = Snapshot(live) });
        await Task.WhenAll(live.Connections.Values
            .Where(c => c.Id != except)
            .Select(async c =>
            {
                try { await c.SendAsync(json); } catch { /* reader loop cleans it up */ }
            }));
    }

    private static string Serialize(object value) => JsonSerializer.Serialize(value, Json);
}
