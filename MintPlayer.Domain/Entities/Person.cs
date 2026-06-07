namespace MintPlayer.Domain.Entities;

/// <summary>
/// A real person in the catalog (band member, songwriter, …). Mirrors the legacy <c>Person</c>.
/// The display name ("FirstName LastName") is computed by the <c>People_Overview</c> index into the
/// <c>VPerson</c> query-type rather than stored here. The bands a person belongs to are derived via
/// a sub-query over <c>Artist.Members</c> (the membership lives on the Artist side).
/// </summary>
public class Person : Subject
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? Born { get; set; }
    public DateTime? Died { get; set; }
}
