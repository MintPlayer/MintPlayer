using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// A single track on a <see cref="Playlist"/>, embedded as an AsDetail row (legacy <c>PlaylistSong</c>).
/// References the <see cref="Song"/> and carries its ordinal <see cref="Index"/> within the playlist
/// (maintained by the drag-reorder renderer). Breadcrumb recurses into the referenced song.
/// </summary>
[Breadcrumb("{SongId}")]
public class PlaylistTrack
{
    [Reference(typeof(Song), "GetSongs")]
    public string? SongId { get; set; }

    /// <summary>Zero-based position within the playlist; preserves track order.</summary>
    public int Index { get; set; }
}
