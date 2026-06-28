using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// A musical artist / band. Mirrors the legacy <c>Artist</c>. Display = <c>[Breadcrumb]</c> "{Name}".
/// Band members are embedded as <see cref="Members"/> (AsDetail); the songs credited to an artist are
/// a sub-query over <c>Song.Artists</c>.
/// </summary>
[Breadcrumb("{Name}")]
public class Artist : Subject
{
    public string Name { get; set; } = string.Empty;
    public int? YearStarted { get; set; }
    public int? YearQuit { get; set; }

    public List<ArtistMember> Members { get; set; } = [];
}
