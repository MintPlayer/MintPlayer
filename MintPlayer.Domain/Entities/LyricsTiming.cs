namespace MintPlayer.Domain.Entities;

/// <summary>
/// Karaoke timing for one recording of a song, embedded on <see cref="Song.LyricsTimings"/>. Keyed by
/// <see cref="MediumUrl"/> — the URL of the <see cref="Medium"/> being timed (matching a
/// <c>Song.Media[].Value</c>) — because embedded media have no stable id of their own, and the URL is
/// exactly what the player reports as "now playing", so the display picks the right timing by matching
/// the currently-playing URL.
///
/// <see cref="StartTimes"/> runs parallel to the lines of <see cref="Song.Lyrics"/> (split on newline):
/// entry <c>i</c> is when line <c>i</c> begins, in seconds; <c>null</c> = that line is unsynced. The
/// player highlights the latest line whose start has passed.
/// </summary>
public class LyricsTiming
{
    public string MediumUrl { get; set; } = string.Empty;

    public List<double?> StartTimes { get; set; } = [];
}
