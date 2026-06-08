using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// A link to a subject on an external platform (Spotify, YouTube, official site, …). Embedded as an
/// <c>AsDetail</c> array on each <see cref="Subject"/> (legacy <c>Medium</c>, which was a separate
/// table; here it lives inline on the owning document).
/// </summary>
public class Medium
{
    /// <summary>The URL / identifier on that medium. Declared first so it's the leading grid column,
    /// where the play-triangle renderer ("media-player") shows a play button when the URL is playable
    /// by the <c>&lt;video-player&gt;</c> component.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>The kind of medium (Spotify, YouTube, …).</summary>
    [Reference(typeof(MediumType), "GetMediumTypes")]
    public string? TypeId { get; set; }
}
