using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace MintPlayer.Web.Rooms;

/// <summary>One open socket in a room. Sends are serialized (a <see cref="WebSocket"/> allows one outstanding send).</summary>
public sealed class RoomConnection(string id, WebSocket socket, string? userId, string displayName)
{
    private readonly SemaphoreSlim sendLock = new(1, 1);

    public string Id { get; } = id;
    public WebSocket Socket { get; } = socket;
    /// <summary>Identity user id; <c>null</c> for an anonymous listener.</summary>
    public string? UserId { get; } = userId;
    public string DisplayName { get; } = displayName;
    public bool SignedIn => UserId is not null;

    public async Task SendAsync(string json)
    {
        await sendLock.WaitAsync();
        try
        {
            if (Socket.State == WebSocketState.Open)
                await Socket.WriteMessage(json); // MintPlayer.Dotnet.SocketExtensions
        }
        finally
        {
            sendLock.Release();
        }
    }
}

/// <summary>
/// The in-memory half of a room: the loaded <see cref="RoomDocument"/>, its live connections and the current
/// item's skip votes. All state mutations happen under <see cref="Gate"/> so a burst of messages from many
/// clients applies in a single order, and each mutation is persisted + broadcast before the gate is released.
/// </summary>
public sealed class LiveRoom(RoomDocument doc)
{
    public RoomDocument Doc { get; } = doc;
    public SemaphoreSlim Gate { get; } = new(1, 1);
    public ConcurrentDictionary<string, RoomConnection> Connections { get; } = new();
    /// <summary>User ids that voted to skip the current item (reset on every item change).</summary>
    public HashSet<string> SkipVotes { get; } = [];

    public bool IsHost(RoomConnection c) => c.UserId is not null && c.UserId == Doc.HostUserId;

    /// <summary>Majority of the distinct signed-in users currently connected (min 1).</summary>
    public int VotesNeeded()
    {
        var signedIn = Connections.Values.Where(c => c.UserId is not null).Select(c => c.UserId).Distinct().Count();
        return Math.Max(1, (int)Math.Ceiling(signedIn / 2d));
    }

    public RoomQueueItem? CurrentItem => Doc.Queue.FirstOrDefault(i => i.Id == Doc.CurrentItemId);
}
