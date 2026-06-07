using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// A link to a subject on an external platform (Spotify, YouTube, official site, …). Embedded as an
/// <c>AsDetail</c> array on each <see cref="Subject"/> (legacy <c>Medium</c>, which was a separate
/// table; here it lives inline on the owning document).
/// </summary>
public class Medium
{
    /// <summary>The kind of medium (Spotify, YouTube, …).</summary>
    [Reference(typeof(MediumType), "GetMediumTypes")]
    public string? TypeId { get; set; }

    /// <summary>The URL / identifier on that medium.</summary>
    public string Value { get; set; } = string.Empty;
}
