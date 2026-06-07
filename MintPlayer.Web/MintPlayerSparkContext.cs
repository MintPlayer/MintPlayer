using MintPlayer.Domain.Entities;
using MintPlayer.Spark;
using Raven.Client.Documents.Linq;

namespace MintPlayer.Web;

/// <summary>
/// The Spark data context for MintPlayer. Each property exposes a RavenDB collection as a
/// queryable; Spark's CRUD middleware and the model synchronizer discover entities through here.
/// Grows one collection at a time as the catalog domain is migrated (Phase 2+).
///
/// Soft-delete convention (step 1.4): every collection here filters out <c>IsDeleted</c> rows,
/// so the named-query / datatable path never surfaces deleted documents. The single-load and
/// delete paths are handled by <c>EntityActions&lt;T&gt;</c>. Raven turns the <c>Where</c> into
/// an auto-index predicate, so this is an index-level filter, not a post-load one.
/// </summary>
public class MintPlayerSparkContext : SparkContext
{
    // Cast: LINQ's Where is typed IQueryable<T>, but Raven's provider returns a RavenQueryInspector<T>
    // (an IRavenQueryable<T>) at runtime, which the model synchronizer + query executor require.
    public IRavenQueryable<MediumType> MediumTypes =>
        (IRavenQueryable<MediumType>)Session.Query<MediumType>().Where(x => !x.IsDeleted);

    public IRavenQueryable<TagCategory> TagCategories =>
        (IRavenQueryable<TagCategory>)Session.Query<TagCategory>().Where(x => !x.IsDeleted);

    public IRavenQueryable<Tag> Tags =>
        (IRavenQueryable<Tag>)Session.Query<Tag>().Where(x => !x.IsDeleted);

    public IRavenQueryable<Person> People =>
        (IRavenQueryable<Person>)Session.Query<Person>().Where(x => !x.IsDeleted);

    public IRavenQueryable<Artist> Artists =>
        (IRavenQueryable<Artist>)Session.Query<Artist>().Where(x => !x.IsDeleted);

    public IRavenQueryable<Song> Songs =>
        (IRavenQueryable<Song>)Session.Query<Song>().Where(x => !x.IsDeleted);
}
