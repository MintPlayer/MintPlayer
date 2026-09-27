using System.Text.RegularExpressions;
using MintPlayer.Domain.Entities;
using Raven.Client.Documents.Session;

namespace MintPlayer.Web.PublicSite;

/// <summary>Public (anonymous) projection of a <see cref="Song"/> for the SSR'd <c>/song/{id}</c> page.</summary>
/// <param name="Id">The RavenDB id without the collection prefix (<c>Songs/313</c> → <c>313</c>) — the URL segment.</param>
/// <param name="Released"><c>yyyy-MM-dd</c> (a calendar date, no timezone), or <c>null</c>.</param>
/// <param name="ModifiedAt">Exact UTC instant (<c>yyyy-MM-ddTHH:mm:ssZ</c>) for JSON-LD <c>dateModified</c> (D33), or <c>null</c>. A string, so the Newtonsoft (prerender) and System.Text.Json (API) paths serialize it identically.</param>
public sealed record PublicSongDto(string Id, string Title, string? Released, IReadOnlyList<PublicArtistRefDto> Artists, string? ModifiedAt);

public sealed record PublicArtistRefDto(string Id, string Name, bool Credited);

/// <summary>
/// Loads the public song view in one RavenDB round-trip (song + included artists). Shared by the
/// prerender path (<see cref="MintPlayerSpaPrerenderingService"/>, no HTTP hop) and the client-side
/// navigation endpoint (<c>GET /api/public/song/{id}</c>) so both produce byte-identical data —
/// a precondition for hydration without mismatches.
/// </summary>
public sealed partial class PublicSongReader
{
    private const string Prefix = "Songs/";
    private readonly IAsyncDocumentSession session;

    public PublicSongReader(IAsyncDocumentSession session)
    {
        this.session = session;
    }

    public async Task<PublicSongDto?> GetAsync(string? urlId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(urlId) || !UrlIdRegex().IsMatch(urlId))
            return null;

        var song = await session
            .Include<Song>(s => s.Artists.Select(a => a.ArtistId))
            .LoadAsync<Song>(Prefix + urlId, cancellationToken);
        if (song is null || song.IsDeleted)
            return null;

        var artistIds = song.Artists.Select(a => a.ArtistId).OfType<string>().Distinct().ToList();
        var artists = artistIds.Count == 0
            ? new Dictionary<string, Artist>()
            : await session.LoadAsync<Artist>(artistIds, cancellationToken); // served from the include, no extra request

        var artistRefs = song.Artists
            .Where(a => a.ArtistId is not null && artists.TryGetValue(a.ArtistId, out var artist) && artist is { IsDeleted: false })
            .Select(a => new PublicArtistRefDto(TrimCollection(a.ArtistId!), artists[a.ArtistId!].Name, a.Credited))
            .ToList();

        return new PublicSongDto(
            urlId,
            song.Title,
            song.Released?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            artistRefs,
            song.ModifiedAt?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", System.Globalization.CultureInfo.InvariantCulture));
    }

    private static string TrimCollection(string id) => id[(id.IndexOf('/') + 1)..];

    /// <summary>Numeric legacy ids (<c>313</c>) and RavenDB HiLo ids (<c>1025-A</c>); nothing that could escape the collection.</summary>
    [GeneratedRegex("^[0-9A-Za-z-]{1,40}$")]
    private static partial Regex UrlIdRegex();
}
