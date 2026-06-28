using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Domain.Entities;
using MintPlayer.Web.Models;
using Raven.Client.Documents.Session;

namespace MintPlayer.Web.Controllers;

/// <summary>
/// Playback helper for persisted <see cref="Playlist"/>s. Resolves a playlist's ordered tracks to the
/// data the global player needs — song id + title + candidate media URLs — in a <b>single round-trip</b>
/// (one batched <see cref="IAsyncDocumentSession.LoadAsync{T}(IEnumerable{string}, CancellationToken)"/>),
/// so the "Play this playlist" button never fires one request per track.
///
/// <para>Read access mirrors the playlist's row-level security (<c>PlaylistActions.IsAllowedAsync</c>):
/// admins, the owner, and anyone for public playlists. A missing or unreadable playlist returns 404 so
/// existence isn't leaked. Playlist ids are RavenDB string ids (they contain '/'), so the id travels as a
/// query param rather than a route segment — the same convention as <see cref="SubjectController"/>.</para>
/// </summary>
[ApiController]
[Route("api/playlist")]
public class PlaylistController : ControllerBase
{
    private readonly IAsyncDocumentSession session;

    public PlaylistController(IAsyncDocumentSession session)
    {
        this.session = session;
    }

    /// <summary>Group membership is a "group" claim valued with the group name (see <c>PlaylistActions</c>).</summary>
    private const string AdministratorGroupName = "Administrator";

    /// <summary>
    /// The playlist's tracks resolved for playback, in order. <c>GET /api/playlist/playable?id={playlistId}</c>.
    /// Tracks whose song is missing, soft-deleted, or has no media are dropped (the player has nothing to load).
    /// </summary>
    [HttpGet("playable")]
    public async Task<ActionResult<IReadOnlyList<PlayablePlaylistTrack>>> Playable([FromQuery] string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("A playlist id is required.");

        var playlist = await session.LoadAsync<Playlist>(id, cancellationToken);
        if (playlist is null || playlist.IsDeleted || !CanRead(playlist))
            return NotFound();

        var songIds = playlist.Tracks
            .Select(t => t.SongId)
            .Where(sid => !string.IsNullOrWhiteSpace(sid))
            .Cast<string>()
            .Distinct()
            .ToList();

        var songs = songIds.Count == 0
            ? new Dictionary<string, Song>()
            : await session.LoadAsync<Song>(songIds, cancellationToken);

        var tracks = new List<PlayablePlaylistTrack>(playlist.Tracks.Count);
        foreach (var track in playlist.Tracks)
        {
            if (track.SongId is null || !songs.TryGetValue(track.SongId, out var song) || song is null || song.IsDeleted)
                continue;

            var urls = song.Media
                .Select(m => m.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();
            if (urls.Count == 0)
                continue;

            tracks.Add(new PlayablePlaylistTrack
            {
                SongId = song.Id!,
                Title = song.Title,
                MediaUrls = urls,
            });
        }

        return Ok(tracks);
    }

    /// <summary>Row-level read check mirroring <c>PlaylistActions.IsAllowedAsync</c>: admin, owner, or public.</summary>
    private bool CanRead(Playlist playlist)
    {
        if (User.HasClaim("group", AdministratorGroupName))
            return true;
        if (playlist.IsPublic)
            return true;

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !string.IsNullOrEmpty(userId) && string.Equals(playlist.OwnerId, userId, StringComparison.Ordinal);
    }
}
