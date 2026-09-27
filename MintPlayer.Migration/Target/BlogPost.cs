using MintPlayer.Domain.Entities;

namespace MintPlayer.Migration.Target;

/// <summary>
/// STOPGAP until F1 adds the real <c>BlogPost</c> entity to MintPlayer.Domain: a minimal record so the
/// 10 legacy posts land in the <c>BlogPosts</c> collection with the F1 field set (Title, Headline, Body,
/// Author → user, Published) plus the <see cref="Entity"/> audit/soft-delete fields. The class name gives
/// the same collection name the domain entity will; once F1 lands, delete this type and map onto the
/// domain entity (the stored <c>Raven-Clr-Type</c> metadata then changes on the next migration run).
/// </summary>
public sealed class BlogPost : Entity
{
    public string Title { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    /// <summary><c>MintPlayerUsers/{guid}</c> of the legacy <c>UserInsertId</c>.</summary>
    public string? AuthorId { get; set; }
    /// <summary>Legacy posts were public on insert: Published = CreatedAt.</summary>
    public DateTimeOffset? Published { get; set; }
}
