using MintPlayer.Spark.Abstractions;

namespace MintPlayer.Domain.Entities;

/// <summary>
/// A real person in the catalog (band member, songwriter, …). Mirrors the legacy <c>Person</c>.
/// The display name is the <c>[Breadcrumb]</c> "FirstName LastName", resolved by the framework's
/// breadcrumb resolver against the collection document (so it renders correctly everywhere — detail
/// header, list, and as a reference, e.g. an <c>ArtistMember</c>'s person column). The <c>VPerson</c>
/// index still provides a searchable/sortable <c>FullName</c>. The bands a person belongs to are
/// derived via a sub-query over <c>Artist.Members</c> (the membership lives on the Artist side).
/// </summary>
[Breadcrumb("{FirstName} {LastName}")]
public class Person : Subject
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? Born { get; set; }
    public DateOnly? Died { get; set; }
}
