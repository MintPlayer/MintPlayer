namespace MintPlayer.Domain.Entities;

/// <summary>
/// The kind of a <c>Medium</c> — e.g. Spotify, YouTube, Apple Music, official website.
/// The simplest catalog entity; used as the first end-to-end vertical slice of the Spark
/// migration (implementation plan step 1.5).
/// </summary>
public class MediumType
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
