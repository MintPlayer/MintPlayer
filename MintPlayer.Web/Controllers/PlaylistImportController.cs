using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Web.Models;
using MintPlayer.Web.YouTube;

namespace MintPlayer.Web.Controllers;

/// <summary>
/// Import a YouTube playlist into a MintPlayer playlist (F16). Signed-in users only (cookie scheme). Like the
/// app's other custom POSTs (<see cref="SubjectController"/> likes, lyrics timings) there's no antiforgery check
/// yet — the hardened public surface is the JWT-bearer API (Phase 6.6). Creating draft songs is a catalog write,
/// so it additionally requires the Editor or Administrator group.
/// </summary>
[ApiController]
[Route("api/playlist/import/youtube")]
[Authorize]
public class PlaylistImportController(YouTubePlaylistImporter importer, UserManager<MintPlayerUser> userManager) : ControllerBase
{
    /// <summary>Match report without writing anything. <c>POST /api/playlist/import/youtube/preview {url}</c>.</summary>
    [HttpPost("preview")]
    public Task<ActionResult<YouTubeImportPreview>> Preview([FromBody] YouTubeImportPreviewRequest request, CancellationToken cancellationToken)
        => Run<YouTubeImportPreview>(async () => Ok(await importer.PreviewAsync(request?.Url, cancellationToken)));

    /// <summary>Create the playlist. <c>POST /api/playlist/import/youtube {url, name, isPublic, createDraftsForUnmatched}</c>.</summary>
    [HttpPost]
    public Task<ActionResult<YouTubeImportResult>> Import([FromBody] YouTubeImportRequest request, CancellationToken cancellationToken)
        => Run<YouTubeImportResult>(async () =>
        {
            var mayCreateDrafts = User.HasClaim("group", "Editor") || User.HasClaim("group", "Administrator");
            var result = await importer.ImportAsync(request ?? new(), userManager.GetUserId(User)!, mayCreateDrafts, cancellationToken);
            return Ok(result);
        });

    private async Task<ActionResult<T>> Run<T>(Func<Task<ActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (YouTubeApiException ex)
        {
            return ex.Reason switch
            {
                "playlistNotFound" => NotFound(ex.Message),
                "notConfigured" => StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message),
                "quotaExceeded" or "dailyLimitExceeded" or "rateLimitExceeded"
                    => StatusCode(StatusCodes.Status429TooManyRequests, "YouTube quota exhausted — try again later."),
                _ => StatusCode(StatusCodes.Status502BadGateway, $"YouTube API error: {ex.Reason}"),
            };
        }
    }
}
