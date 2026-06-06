using MintPlayer.Domain.Entities;
using MintPlayer.Spark;
using Raven.Client.Documents.Linq;

namespace MintPlayer.Web;

/// <summary>
/// The Spark data context for MintPlayer. Each property exposes a RavenDB collection as a
/// queryable; Spark's CRUD middleware and the model synchronizer discover entities through here.
/// Grows one collection at a time as the catalog domain is migrated (Phase 2+).
/// </summary>
public class MintPlayerSparkContext : SparkContext
{
    public IRavenQueryable<MediumType> MediumTypes => Session.Query<MediumType>();
}
