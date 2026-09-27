using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// A person's membership in an artist/band, embedded as an AsDetail row on <see cref="Artist"/>
/// (legacy <c>ArtistPerson</c>). The membership lives on the Artist side; a person's bands are a
/// sub-query over these rows. Breadcrumb recurses into the referenced person (renders their name).
/// </summary>
[Breadcrumb("{PersonId}")]
public class ArtistMember
{
    [Reference(typeof(Person), "GetPeople")]
    public string? PersonId { get; set; }

    /// <summary>Whether the person is a current member.</summary>
    public bool Active { get; set; }
}
