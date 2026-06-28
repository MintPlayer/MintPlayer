using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// Shared base for the catalog's taggable, media-bearing entities — <see cref="Person"/>,
/// <c>Artist</c>, <c>Song</c> (legacy <c>Subject</c>). Carries the cross-cutting catalog data on top
/// of the <see cref="Entity"/> conventions (audit + soft-delete):
///  - <see cref="Media"/>: external links (Spotify/YouTube/…), embedded as an AsDetail array;
///  - <see cref="TagIds"/>: a list of references to <see cref="Tag"/> documents — a native Spark
///    multi-reference (rendered with the built-in searchable multi-select).
/// Likes are modelled separately (their own collection + aggregate index) in a later slice.
/// </summary>
public abstract class Subject : Entity
{
    public List<Medium> Media { get; set; } = [];

    /// <summary>Ids of the <see cref="Tag"/>s attached to this subject (multi-reference).</summary>
    [Reference(typeof(Tag), "GetTags")]
    public List<string> TagIds { get; set; } = [];
}
