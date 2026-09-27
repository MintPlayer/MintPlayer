namespace MintPlayer.Web.Rooms;

/// <summary>
/// A listen-together room (F13, spike S9). Persisted as a plain RavenDB document (<c>Rooms/{id}</c>) — not a
/// Spark entity: rooms are never edited through the auto-UI, only through the room socket. The document is
/// the durable half of the room (survives an app restart / redeploy); connections and skip votes are the
/// in-memory half (<see cref="LiveRoom"/>).
///
/// <para>Playback is stored as an <see cref="RoomAnchor"/> rather than a ticking position: "at server time T
/// the current item was at P and was (not) playing". Any later position is derived from it, so nothing has to
/// be written while a track plays, and a room reloaded after a restart resumes at the wall-clock-correct
/// position.</para>
/// </summary>
public sealed class RoomDocument
{
    /// <summary><c>Rooms/{token}</c>, where the token is 128 random bits, base64url (unguessable, D31).</summary>
    public string Id { get; set; } = "";

    public string Name { get; set; } = "";

    /// <summary>Identity user id of the host. Only a signed-in user can host (D31).</summary>
    public string HostUserId { get; set; } = "";
    public string HostName { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; }
    /// <summary>Last state change; basis for inactivity expiry.</summary>
    public DateTimeOffset LastActivityAt { get; set; }

    /// <summary>Host closed the room to NEW listeners (existing connections stay).</summary>
    public bool Locked { get; set; }

    /// <summary>Host ended the room; the socket refuses joins.</summary>
    public bool Closed { get; set; }

    /// <summary>The shared queue (played items stay in place as history; <see cref="CurrentItemId"/> points into it).</summary>
    public List<RoomQueueItem> Queue { get; set; } = [];

    public string? CurrentItemId { get; set; }

    public RoomAnchor Anchor { get; set; } = new();
}

public sealed class RoomQueueItem
{
    public string Id { get; set; } = "";
    public string Url { get; set; } = "";
    public string Title { get; set; } = "";
    public string AddedBy { get; set; } = "";
}

/// <summary>Playback anchor: at <see cref="ServerTimeMs"/> the item <see cref="MediaUrl"/> was at <see cref="PositionSec"/>.</summary>
public sealed class RoomAnchor
{
    public string? MediaUrl { get; set; }
    public double PositionSec { get; set; }
    /// <summary>Server clock, Unix epoch milliseconds.</summary>
    public long ServerTimeMs { get; set; }
    public bool Playing { get; set; }

    public double PositionAt(long nowMs)
        => Playing ? PositionSec + (nowMs - ServerTimeMs) / 1000d : PositionSec;
}
