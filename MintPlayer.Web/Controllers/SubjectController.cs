using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Domain.Entities;
using MintPlayer.Web.Indexes;
using MintPlayer.Web.Models;
using MintPlayer.Web.Projections;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace MintPlayer.Web.Controllers;

/// <summary>
/// Likes / favorites for catalog subjects (Phase 2.6) — the RavenDB port of the legacy subject like API.
/// Likes live as one <see cref="UserLike"/> document per user (two id arrays); per-subject totals come from
/// the <see cref="Likes_Count"/> fan-out map-reduce index. Subject ids are RavenDB string ids (they contain
/// '/'), so they travel as query/body params rather than route segments.
///
/// <para>Auth: reading totals is anonymous (public). Writing a like and listing favorites require a signed-in
/// user (cookie scheme). The state-changing POST is currently cookie-authenticated without an antiforgery
/// check — acceptable mid-migration; the hardened public surface is the JWT-bearer API in Phase 6.6
/// (CSRF-immune). The public site's like widget (Phase 4.3) consumes these endpoints.</para>
/// </summary>
[ApiController]
[Route("api/subject")]
public class SubjectController : ControllerBase
{
    private readonly IAsyncDocumentSession session;
    private readonly UserManager<MintPlayerUser> userManager;

    public SubjectController(IAsyncDocumentSession session, UserManager<MintPlayerUser> userManager)
    {
        this.session = session;
        this.userManager = userManager;
    }

    private static string DocId(string userId) => $"UserLikes/{userId}";

    /// <summary>
    /// Like totals for a subject, plus the current user's own preference when signed in.
    /// <c>GET /api/subject/likes?id={subjectId}</c>. Anonymous.
    /// </summary>
    [HttpGet("likes")]
    public async Task<ActionResult<SubjectLikeResult>> GetLikes([FromQuery] string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("A subject id is required.");

        var counts = await session.Query<LikeCount, Likes_Count>()
            .Where(x => x.SubjectId == id)
            .FirstOrDefaultAsync(cancellationToken);

        var result = new SubjectLikeResult
        {
            Likes = counts?.Likes ?? 0,
            Dislikes = counts?.Dislikes ?? 0,
            Authenticated = User.Identity?.IsAuthenticated == true,
        };

        if (result.Authenticated)
        {
            var doc = await session.LoadAsync<UserLike>(DocId(userManager.GetUserId(User)!), cancellationToken);
            if (doc is not null)
                result.Like = doc.Likes.Contains(id) ? true
                            : doc.Dislikes.Contains(id) ? false
                            : null;
        }

        return Ok(result);
    }

    /// <summary>
    /// Set the current user's preference for a subject (like / dislike / clear) and return the fresh totals.
    /// <c>POST /api/subject/likes</c>. Requires authentication.
    /// </summary>
    [HttpPost("likes")]
    [Authorize]
    public async Task<ActionResult<SubjectLikeResult>> SetLike([FromBody] SetLikeRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.SubjectId))
            return BadRequest("A subject id is required.");

        var userId = userManager.GetUserId(User)!;
        var docId = DocId(userId);

        var doc = await session.LoadAsync<UserLike>(docId, cancellationToken);
        if (doc is null)
        {
            doc = new UserLike { Id = docId, UserId = userId };
            await session.StoreAsync(doc, docId, cancellationToken);
        }

        // A subject sits in at most one array — drop any prior state, then apply the new one.
        doc.Likes.Remove(request.SubjectId);
        doc.Dislikes.Remove(request.SubjectId);
        if (request.Like == true)
            doc.Likes.Add(request.SubjectId);
        else if (request.Like == false)
            doc.Dislikes.Add(request.SubjectId);
        // request.Like == null → cleared (unlike), nothing to add.

        await session.SaveChangesAsync(cancellationToken);

        // Wait for the map-reduce index to catch up so the returned totals reflect this write
        // (the index is otherwise eventually consistent).
        var counts = await session.Query<LikeCount, Likes_Count>()
            .Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(10)))
            .Where(x => x.SubjectId == request.SubjectId)
            .FirstOrDefaultAsync(cancellationToken);

        return Ok(new SubjectLikeResult
        {
            Likes = counts?.Likes ?? 0,
            Dislikes = counts?.Dislikes ?? 0,
            Like = request.Like,
            Authenticated = true,
        });
    }

    /// <summary>
    /// The current user's favorites — the subjects they like — resolved to id / type / display text via the
    /// shared <see cref="Subjects_Search"/> projection. <c>GET /api/subject/favorites</c>. Requires authentication.
    /// </summary>
    [HttpGet("favorites")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<VSubject>>> Favorites(CancellationToken cancellationToken)
    {
        var doc = await session.LoadAsync<UserLike>(DocId(userManager.GetUserId(User)!), cancellationToken);
        if (doc is null || doc.Likes.Count == 0)
            return Ok(Array.Empty<VSubject>());

        var favorites = await session.Query<VSubject, Subjects_Search>()
            .Where(x => x.Id.In(doc.Likes))
            .ProjectInto<VSubject>()
            .ToListAsync(cancellationToken);

        return Ok(favorites);
    }
}
