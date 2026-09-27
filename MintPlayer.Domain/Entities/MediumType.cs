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
    /// <summary>
    /// Whether media of this type are shown to ordinary viewers (legacy <c>MediumTypes.Visible</c>, D15).
    /// Hidden types — e.g. legacy type 15 "Songteksten", which holds the genius.com lyric-source links —
    /// are kept as data but shown only to privileged users. Defaults to <c>true</c> for new types.
    /// </summary>
    public bool Visible { get; set; } = true;
}
