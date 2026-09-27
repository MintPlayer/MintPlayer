using MintPlayer.Domain.Entities;

namespace MintPlayer.Web.Models;

/// <summary>
/// A song's lyrics for the karaoke UI: the canonical text plus per-recording timing, and whether the
/// caller may curate timing (Editor/Administrator). The text itself is edited through the Spark song
/// form, not here.
/// </summary>
public class SongLyricsResult
{
    public string Text { get; set; } = string.Empty;
    public List<LyricsTiming> Timings { get; set; } = [];
    public bool CanEdit { get; set; }
}

/// <summary>Body of <c>PUT /api/song/lyrics/timings</c>: replace a song's karaoke timing wholesale.</summary>
public class SetLyricsTimingsRequest
{
    public string SongId { get; set; } = string.Empty;
    public List<LyricsTiming> Timings { get; set; } = [];
}
