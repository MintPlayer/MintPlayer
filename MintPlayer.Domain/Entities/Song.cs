using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// A song. Mirrors the legacy <c>Song</c>. Display = <c>[Breadcrumb]</c> "{Title}".
/// Crediting artists are embedded as <see cref="Artists"/> (AsDetail); media + tags come from
/// <see cref="Subject"/>. Lyrics + playlist membership are modelled in Phase 3.
/// </summary>
[Breadcrumb("{Title}")]
public class Song : Subject
{
    public string Title { get; set; } = string.Empty;
    public DateOnly? Released { get; set; }

    public List<SongArtist> Artists { get; set; } = [];
}
