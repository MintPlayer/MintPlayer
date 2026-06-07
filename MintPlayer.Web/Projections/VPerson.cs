using MintPlayer.Spark.Abstractions;
using MintPlayer.Web.Indexes;

namespace MintPlayer.Web.Projections;

/// <summary>
/// Query-type (list view) for <see cref="MintPlayer.Domain.Entities.Person"/>, produced by the
/// <see cref="People_Overview"/> index. Holds the computed <c>FullName</c> (also the display
/// attribute + full-text search field). The model synchronizer merges these fields into
/// <c>Person.json</c>: <c>FullName</c> shows on list/query views, while the editable
/// <c>FirstName</c>/<c>LastName</c>/media/tags (only on the collection type) show on detail/edit.
/// </summary>
[FromIndex(typeof(People_Overview))]
public class VPerson
{
    public string? Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? Born { get; set; }
    public DateTime? Died { get; set; }
}
