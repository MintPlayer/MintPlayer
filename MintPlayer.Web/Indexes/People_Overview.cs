using MintPlayer.Domain.Entities;
using MintPlayer.Web.Projections;
using Raven.Client.Documents.Indexes;

namespace MintPlayer.Web.Indexes;

/// <summary>
/// Projects <see cref="Person"/> documents into the <see cref="VPerson"/> query-type: computes
/// the display <c>FullName</c> and excludes soft-deleted rows (the projected list path queries the
/// index directly, bypassing the SparkContext's <c>!IsDeleted</c> filter — so the filter lives here).
/// Auto-registered as a RavenDB index at startup (scanned from the MintPlayer.Web assembly).
/// </summary>
public class People_Overview : AbstractIndexCreationTask<Person>
{
    public People_Overview()
    {
        Map = people => from person in people
                        where !person.IsDeleted
                        select new VPerson
                        {
                            Id = person.Id,
                            FullName = person.FirstName + " " + person.LastName,
                            Born = person.Born,
                            Died = person.Died,
                        };

        Index(nameof(VPerson.FullName), FieldIndexing.Search);
        StoreAllFields(FieldStorage.Yes);
    }
}
