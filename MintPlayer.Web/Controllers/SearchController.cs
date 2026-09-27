using Microsoft.AspNetCore.Mvc;
using MintPlayer.Web.Indexes;
using MintPlayer.Web.Projections;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Queries.Suggestions;
using Raven.Client.Documents.Session;

namespace MintPlayer.Web.Controllers;

/// <summary>
/// Unified cross-catalog search (Phase 2.5). Queries the <see cref="Subjects_Search"/> multi-map index
/// — RavenDB full text over artists, people and songs in one shot — and exposes the typo-tolerant
/// autocomplete via RavenDB's <c>SuggestUsing</c>. This is the RavenDB-native replacement for the legacy
/// Elasticsearch search (D1); the public search page (Phase 4.4) consumes these endpoints.
///
/// <para>Anonymous on purpose: search is public. Spark's group authorization governs only its own
/// <c>/spark/*</c> endpoints, not these MVC controllers, so no <c>[Authorize]</c> means open read.
/// Lives under <c>/api</c>, which is excluded from the SPA fallback in <c>Program.cs</c>.</para>
/// </summary>
[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly IAsyncDocumentSession session;

    public SearchController(IAsyncDocumentSession session)
    {
        this.session = session;
    }

    /// <summary>
    /// Full-text search across the catalog. <paramref name="q"/> matches the subject's display text
    /// (artist/song name, person full name); a trailing wildcard makes it match-as-you-type on the last
    /// token. Optionally narrow to one <paramref name="type"/> (<c>Artist</c>/<c>Person</c>/<c>Song</c>).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VSubject>>> Search(
        [FromQuery] string? q,
        [FromQuery] string? type,
        [FromQuery] int take = 25,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(Array.Empty<VSubject>());

        take = Math.Clamp(take, 1, 100);

        // One Search clause matching the whole term OR the term as a prefix (so results appear while
        // typing). Kept to a single clause on purpose: a second Or'd Search would leak its Or into the
        // boolean tree and silently drop the SubjectType filter below.
        var term = q.Trim();
        var query = session.Query<VSubject, Subjects_Search>()
            .Search(x => x.Text, $"{term} {term}*");

        if (!string.IsNullOrWhiteSpace(type))
            // Raven's Where is typed IQueryable<T> but returns an IRavenQueryable<T> at runtime.
            query = (IRavenQueryable<VSubject>)query.Where(x => x.SubjectType == type);

        // ProjectInto reads the stored index fields (SubjectType/Text are index-computed, not on the
        // source docs) instead of loading the underlying Person/Artist/Song documents.
        var results = await query.Take(take).ProjectInto<VSubject>().ToListAsync(cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Typo-tolerant autocomplete suggestions for the search box, via RavenDB <c>SuggestUsing</c> over
    /// the indexed <c>Text</c> field. Returns a flat list of suggested terms.
    /// </summary>
    [HttpGet("suggest")]
    public async Task<ActionResult<IReadOnlyList<string>>> Suggest(
        [FromQuery] string? q,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(Array.Empty<string>());

        take = Math.Clamp(take, 1, 25);

        var suggestions = await session.Query<VSubject, Subjects_Search>()
            .SuggestUsing(builder => builder
                .ByField(x => x.Text, q)
                .WithOptions(new SuggestionOptions { PageSize = take, SortMode = SuggestionSortMode.Popularity }))
            .ExecuteAsync(cancellationToken);

        return Ok(suggestions[nameof(VSubject.Text)].Suggestions);
    }
}
