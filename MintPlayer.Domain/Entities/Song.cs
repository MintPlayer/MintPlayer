namespace MintPlayer.Domain.Entities;

/// <summary>
/// A song. Mirrors the legacy <c>Song</c>. <see cref="Title"/> is the display value directly.
/// Crediting artists are embedded as <see cref="Artists"/> (AsDetail); media + tags come from
/// <see cref="Subject"/>. Lyrics + playlist membership are modelled in Phase 3.
/// </summary>
public class Song : Subject
{
    public string Title { get; set; } = string.Empty;
    public DateTime? Released { get; set; }

    public List<SongArtist> Artists { get; set; } = [];
}
