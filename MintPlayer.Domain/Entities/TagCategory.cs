using System.Drawing;
using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// A grouping of <see cref="Tag"/>s (e.g. "Genre", "Mood"), with a colour used to render its tags.
/// Mirrors the legacy <c>TagCategory</c>; its child tags are fetched via a sub-query rather than an
/// embedded collection (RavenDB references). <see cref="Color"/> is a <see cref="System.Drawing.Color"/>
/// (Spark's <c>color</c> dataType — round-tripped by the framework's Newtonsoft converter).
/// </summary>
[Breadcrumb("{Description}")]
public class TagCategory : Entity
{
    public string Description { get; set; } = string.Empty;
    public Color Color { get; set; }
}
