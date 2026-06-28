using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// The kind of a <c>Medium</c> — e.g. Spotify, YouTube, Apple Music, official website.
/// The simplest catalog entity; used as the first end-to-end vertical slice of the Spark
/// migration (implementation plan step 1.5) and the first adopter of the shared
/// <see cref="Entity"/> base (audit timestamps, soft delete, legacy <c>OldId</c>).
/// </summary>
[Breadcrumb("{Name}")]
public class MediumType : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
