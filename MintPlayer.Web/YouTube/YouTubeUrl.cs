using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MintPlayer.Web.YouTube;

/// <summary>
/// Canonical YouTube identifiers from the many URL shapes users paste and the catalog stores
/// (<c>www./m./music.youtube.com/watch?v=</c>, <c>youtu.be/</c>, <c>/embed/</c>, <c>/shorts/</c>, <c>/live/</c>,
/// <c>/v/</c>, <c>youtube-nocookie.com</c>, extra params like <c>&amp;ab_channel=</c>, <c>&amp;t=</c>, <c>?si=</c>).
///
/// <para><see cref="VideoIdPattern"/> is the single source of truth: the <c>Songs_ByYouTubeId</c> RavenDB index
/// evaluates the very same pattern server-side, so "what the index projects" and "what the importer looks up"
/// can never drift apart.</para>
/// </summary>
public static partial class YouTubeUrl
{
    /// <summary>
    /// Matches a YouTube video URL anywhere in a string; group 1 is the 11-char video id (case preserved).
    /// The host must not be preceded by a letter/digit/hyphen (rejects <c>notyoutube.com</c>), the id must be
    /// exactly 11 chars (not followed by another id char). Inline <c>(?i)</c> so the index needs no options arg.
    /// </summary>
    public const string VideoIdPattern =
        @"(?i)(?<![a-z0-9-])(?:youtube(?:-nocookie)?\.com/(?:watch/?\?(?:[^#\s]*?&)?v=|embed/|shorts/|live/|v/|e/)|youtu\.be/)([a-z0-9_-]{11})(?![a-z0-9_-])";

    [GeneratedRegex(VideoIdPattern)]
    private static partial Regex VideoIdRegex();

    [GeneratedRegex(@"^[A-Za-z0-9_-]{11}$")]
    private static partial Regex BareVideoIdRegex();

    [GeneratedRegex(@"^[A-Za-z0-9_-]{12,64}$")]
    private static partial Regex PlaylistIdRegex();

    /// <summary>Canonical watch URL stored on draft songs.</summary>
    public static string WatchUrl(string videoId) => $"https://www.youtube.com/watch?v={videoId}";

    /// <summary>The canonical video id of a YouTube URL (or a bare 11-char id); null when it isn't one.</summary>
    public static string? GetVideoId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var trimmed = value.Trim();
        if (BareVideoIdRegex().IsMatch(trimmed))
            return trimmed;
        var match = VideoIdRegex().Match(trimmed);
        return match.Success ? match.Groups[1].Value : null;
    }

    public static bool TryGetVideoId(string? value, [NotNullWhen(true)] out string? videoId)
        => (videoId = GetVideoId(value)) is not null;

    /// <summary>
    /// The playlist id from any playlist-bearing URL — <c>youtube.com/playlist?list=</c>,
    /// <c>watch?v=…&amp;list=</c>, <c>music.youtube.com/playlist?list=</c>, <c>youtu.be/{id}?list=</c>,
    /// <c>/embed/videoseries?list=</c> — or a bare playlist id (PL…, OLAK5uy_…, UU…). Null otherwise.
    /// </summary>
    public static string? GetPlaylistId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var trimmed = value.Trim();

        if (PlaylistIdRegex().IsMatch(trimmed))
            return trimmed;

        if (!trimmed.Contains("://", StringComparison.Ordinal))
            trimmed = "https://" + trimmed;
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || !IsYouTubeHost(uri.Host))
            return null;

        foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = pair.IndexOf('=');
            if (eq <= 0 || !pair.AsSpan(0, eq).Equals("list", StringComparison.OrdinalIgnoreCase))
                continue;
            var id = Uri.UnescapeDataString(pair[(eq + 1)..]);
            return PlaylistIdRegex().IsMatch(id) ? id : null;
        }
        return null;
    }

    public static bool TryGetPlaylistId(string? value, [NotNullWhen(true)] out string? playlistId)
        => (playlistId = GetPlaylistId(value)) is not null;

    /// <summary>
    /// Auto-generated "mix"/radio lists (RD…) are not readable through the Data API (playlistNotFound).
    /// (The personal LL / WL lists are too short to pass <see cref="GetPlaylistId"/> in the first place.)
    /// </summary>
    public static bool IsUnsupportedPlaylist(string playlistId)
        => playlistId.StartsWith("RD", StringComparison.Ordinal);

    private static bool IsYouTubeHost(string host)
    {
        host = host.ToLowerInvariant();
        return host is "youtu.be" or "youtube.com" or "youtube-nocookie.com"
            || host.EndsWith(".youtube.com", StringComparison.Ordinal)
            || host.EndsWith(".youtube-nocookie.com", StringComparison.Ordinal);
    }
}
