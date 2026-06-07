using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// An artist credited on a song, embedded as an AsDetail row on <see cref="Song"/> (legacy
/// <c>ArtistSong</c>). The link lives on the Song side; an artist's songs are a sub-query over these rows.
/// </summary>
public class SongArtist
{
    [Reference(typeof(Artist), "GetArtists")]
    public string? ArtistId { get; set; }

    /// <summary>Whether the artist is officially credited (vs. featured/uncredited).</summary>
    public bool Credited { get; set; }
}
