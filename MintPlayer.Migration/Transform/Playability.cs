using System.Text.RegularExpressions;

namespace MintPlayer.Migration.Transform;

/// <summary>
/// Server-side mirror of the client's playability check (<c>MediaPlayabilityService</c> →
/// <c>findApis(url, VIDEO_PLAYER_PLUGINS)</c>). The karaoke display matches a <c>LyricsTiming</c> by the
/// URL the player is playing, and <c>PlaylistPlaybackService.firstPlayable</c> plays the first playable
/// URL of <c>Song.Media</c> in array order — so a migrated timeline is keyed to that URL.
/// The regexes are copied verbatim from the plugin api services' <c>urlRegexes</c>
/// (@mintplayer youtube/vimeo/dailymotion/soundcloud/spotify/file-player, as installed in ClientApp);
/// unanchored unless the source regex is anchored, like <c>RegExp.exec</c>.
/// </summary>
public static class Playability
{
    private static readonly Regex[] Patterns =
    [
        // youtube
        new(@"http[s]{0,1}://(www\.){0,1}youtube\.com/watch\?v=(?<id>[^&]+)"),
        new(@"http[s]{0,1}://m\.youtube\.com/watch\?v=(?<id>[^&]+)"),
        new(@"http[s]{0,1}://(www\.){0,1}youtu\.be/(?<id>[^&?]+)"),
        new(@"http[s]{0,1}://(www\.){0,1}youtube\.com/shorts/(?<id>[^&?]+)"),
        new(@"http[s]{0,1}://m\.youtube\.com/shorts/(?<id>[^&?]+)"),
        new(@"http[s]{0,1}://(www\.){0,1}youtube\.com/live/(?<id>[^&?]+)"),
        // vimeo
        new(@"http[s]{0,1}://(www\.){0,1}vimeo\.com/(?<id>[0-9]+)$"),
        // dailymotion
        new(@"http[s]{0,1}://(www\.){0,1}dailymotion\.com/video/(?<id>[0-9A-Za-z]+)$"),
        // soundcloud
        new(@"(?<id>http[s]{0,1}://(www\.){0,1}soundcloud\.com/.+)$"),
        // spotify
        new(@"http[s]{0,1}://open\.spotify\.com/(?<type>track|episode)/(?<id>[^&/]+)"),
        new(@"spotify:(?<type>track|episode):(?<id>[0-9A-Za-z]+)"),
        // file
        new(@"(?<id>https?://.+\.(?<audiotype>m4a|m4b|mp4a|mpga|mp2|mp2a|mp3|m2a|m3a|wav|weba|aac|oga|spx))"),
        new(@"(?<id>https?://.+\.(?<videotype>mp4|og[gv]|webm|mov|m4v))"),
    ];

    public static bool IsPlayable(string? url)
        => !string.IsNullOrEmpty(url) && Patterns.Any(p => p.IsMatch(url));

    public static string? FirstPlayable(IEnumerable<string> urls) => urls.FirstOrDefault(IsPlayable);
}
