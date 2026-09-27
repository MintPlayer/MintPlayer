using MintPlayer.Domain.Entities;
using MintPlayer.Spark.Queries;
using MintPlayer.Spark.Services;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace MintPlayer.Web.Actions;

/// <summary>
/// Actions for <see cref="Artist"/>. Inherits audit + soft-delete, and exposes a parent-scoped
/// sub-query of the bands a person belongs to (shown on the Person detail page via
/// <c>Person.json</c>'s persistentObject.queries).
/// </summary>
public class ArtistActions : EntityActions<Artist>
{
    private readonly IAsyncDocumentSession session;

    public ArtistActions(IEntityMapper entityMapper, IAsyncDocumentSession session) : base(entityMapper)
    {
        this.session = session;
    }

    /// <summary>Artists whose members include the parent <see cref="Person"/>. Source: "Custom.Person_Artists".</summary>
    public IRavenQueryable<Artist> Person_Artists(CustomQueryArgs args)
    {
        args.EnsureParent("Person");
        return (IRavenQueryable<Artist>)session.Query<Artist>()
            .Where(a => !a.IsDeleted && a.Members.Any(m => m.PersonId == args.Parent!.Id));
    }
}
