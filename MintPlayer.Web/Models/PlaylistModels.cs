namespace MintPlayer.Web.Models;

/// <summary>
/// One track of a playlist resolved for playback: the referenced song's id (queue identity + deep-link
/// target), its display title, and the candidate media URLs in stored order. The client picks the first
/// URL its <c>&lt;video-player&gt;</c> can actually play — playability is a client-side concern (see
/// <c>MediaPlayabilityService</c>, which reuses the player's own URL detection) so the server stays out
/// of the provider-plugin business and just hands over every candidate in one response.
/// </summary>
public sealed class PlayablePlaylistTrack
{
    public required string SongId { get; init; }
    public required string Title { get; init; }
    public required IReadOnlyList<string> MediaUrls { get; init; }
}
