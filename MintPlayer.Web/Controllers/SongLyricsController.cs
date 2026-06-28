using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Domain.Entities;
using MintPlayer.Web.Models;
using Raven.Client.Documents.Session;

namespace MintPlayer.Web.Controllers;

/// <summary>
/// Karaoke lyrics for a song. The lyrics <b>text</b> is curated through the standard Spark song form (a
/// <c>MultiLineString</c> attribute, like the rest of the catalog); this controller serves the read model
/// for the public karaoke display and lets trusted users (Editor / Administrator) capture per-recording
/// line <b>timing</b> — which can't live in a form because it is recorded live against playback. Timing
/// writes touch only <see cref="Song.LyricsTimings"/>; saving creates a Songs revision (revisions enabled
/// in <c>Program.cs</c>). Song ids contain '/', so they travel as a query/body param.
/// </summary>
[ApiController]
[Route("api/song")]
public class SongLyricsController : ControllerBase
{
    private readonly IAsyncDocumentSession session;

    public SongLyricsController(IAsyncDocumentSession session)
    {
        this.session = session;
    }

    private const string AdministratorGroupName = "Administrator";
    private const string EditorGroupName = "Editor";

    /// <summary>Whether the caller may curate lyrics timing (a trusted Editor, or an Administrator).</summary>
    private bool CanEdit =>
        User.HasClaim("group", AdministratorGroupName) || User.HasClaim("group", EditorGroupName);

    /// <summary>
    /// A song's lyrics text + karaoke timing for display, plus whether the caller may edit timing.
    /// <c>GET /api/song/lyrics?id={songId}</c>. Anonymous (counts/timing are public; editing is gated below).
    /// </summary>
    [HttpGet("lyrics")]
    public async Task<ActionResult<SongLyricsResult>> GetLyrics([FromQuery] string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("A song id is required.");

        var song = await session.LoadAsync<Song>(id, cancellationToken);
        if (song is null || song.IsDeleted)
            return NotFound();

        return Ok(new SongLyricsResult
        {
            Text = song.Lyrics ?? string.Empty,
            Timings = song.LyricsTimings,
            CanEdit = CanEdit,
        });
    }

    /// <summary>
    /// Replace a song's karaoke timing. <c>PUT /api/song/lyrics/timings</c>. Editor / Administrator only
    /// (the lyrics text is edited via the song form). Returns the stored timing.
    /// </summary>
    [HttpPut("lyrics/timings")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<LyricsTiming>>> SetTimings([FromBody] SetLyricsTimingsRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.SongId))
            return BadRequest("A song id is required.");
        if (!CanEdit)
            return Forbid();

        var song = await session.LoadAsync<Song>(request.SongId, cancellationToken);
        if (song is null || song.IsDeleted)
            return NotFound();

        song.LyricsTimings = request.Timings ?? [];
        await session.SaveChangesAsync(cancellationToken);

        return Ok(song.LyricsTimings);
    }
}
