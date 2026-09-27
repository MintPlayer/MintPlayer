using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// A single track on a <see cref="Playlist"/>, embedded as an AsDetail row (legacy <c>PlaylistSong</c>).
/// References the <see cref="Song"/>; track order is the position within <see cref="Playlist.Tracks"/>
/// (maintained by Spark's <c>[Sortable]</c> drag-reorder — no explicit index field). Breadcrumb
/// recurses into the referenced song.
/// </summary>
[Breadcrumb("{SongId}")]
public class PlaylistTrack
{
    [Reference(typeof(Song), "GetSongs")]
    public string? SongId { get; set; }
}
