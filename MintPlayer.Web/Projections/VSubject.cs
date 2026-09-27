namespace MintPlayer.Web.Projections;

/// <summary>
/// Cross-collection search projection over the catalog's <see cref="MintPlayer.Domain.Entities.Subject"/>
/// types — <c>Artist</c>, <c>Person</c>, <c>Song</c> — produced by the
/// <see cref="MintPlayer.Web.Indexes.Subjects_Search"/> multi-map index. One shape for results from all
/// three collections, so a single full-text query (and a single autocomplete) spans the whole catalog.
///
/// Unlike <c>VPerson</c> this is intentionally <b>not</b> a Spark query-type (<c>[FromIndex]</c>): it
/// fans across three collections, so it has no single owning PersistentObject. It's consumed directly by
/// the custom <c>SearchController</c> (the unified search/suggest endpoints, per D1 — RavenDB replacing
/// Elasticsearch), which is what the public search page (Phase 4.4) will call.
/// </summary>
public class VSubject
{
    public string? Id { get; set; }

    /// <summary>Which collection this hit came from: <c>"Artist"</c>, <c>"Person"</c> or <c>"Song"</c>.</summary>
    public string SubjectType { get; set; } = string.Empty;

    /// <summary>The display + full-text-searchable text (artist/song name, person full name).</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Ids of the tags on the subject — lets the search be filtered/faceted by tag.</summary>
    public List<string> TagIds { get; set; } = [];
}
