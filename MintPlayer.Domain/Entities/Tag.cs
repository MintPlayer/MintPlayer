using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// A catalog tag belonging to a <see cref="TagCategory"/>, optionally nested under a parent tag
/// (self-reference). Mirrors the legacy <c>Tag</c>; children are fetched via a sub-query rather than
/// an embedded collection. Tags are attached to catalog subjects (Artist/Person/Song) in later slices.
/// </summary>
public class Tag : Entity
{
    public string Description { get; set; } = string.Empty;

    /// <summary>The category this tag belongs to.</summary>
    [Reference(typeof(TagCategory), "GetTagCategories")]
    public string? CategoryId { get; set; }

    /// <summary>Optional parent tag for hierarchical tags (self-reference).</summary>
    [Reference(typeof(Tag), "GetTags")]
    public string? ParentId { get; set; }
}
