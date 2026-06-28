using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// A song. Mirrors the legacy <c>Song</c>. Display = <c>[Breadcrumb]</c> "{Title}".
/// Crediting artists are embedded as <see cref="Artists"/> (AsDetail); media + tags come from
/// <see cref="Subject"/>.
/// </summary>
[Breadcrumb("{Title}")]
public class Song : Subject
{
    public string Title { get; set; } = string.Empty;
    public DateOnly? Released { get; set; }

    public List<SongArtist> Artists { get; set; } = [];

    /// <summary>
    /// The lyrics text, newline-delimited (one line per row). Edited in the standard PO edit form as a
    /// <c>MultiLineString</c> (rendered as a textarea — see <c>Song.json</c>); like the rest of the
    /// catalog it is Editor/Administrator-curated. <c>null</c> until lyrics are added.
    /// </summary>
    public string? Lyrics { get; set; }

    /// <summary>
    /// Per-recording karaoke timing for <see cref="Lyrics"/>: one <see cref="LyricsTiming"/> per medium
    /// whose lines have been timed. Captured live by the karaoke sync tool (a dedicated API, not the PO
    /// form, since it needs playback), so this attribute is hidden in the auto-UI.
    /// </summary>
    public List<LyricsTiming> LyricsTimings { get; set; } = [];
}
