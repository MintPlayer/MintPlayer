using MintPlayer.Domain.Entities;
using MintPlayer.Spark.Queries;
using MintPlayer.Spark.Services;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace MintPlayer.Web.Actions;

/// <summary>
/// Actions for <see cref="Song"/>. Inherits audit + soft-delete, and exposes a parent-scoped
/// sub-query of the songs credited to an artist (shown on the Artist detail page via
/// <c>Artist.json</c>'s persistentObject.queries).
/// </summary>
public class SongActions : EntityActions<Song>
{
    private readonly IAsyncDocumentSession session;

    public SongActions(IEntityMapper entityMapper, IAsyncDocumentSession session) : base(entityMapper)
    {
        this.session = session;
    }

    /// <summary>Songs crediting the parent <see cref="Artist"/>. Source: "Custom.Artist_Songs".</summary>
    public IRavenQueryable<Song> Artist_Songs(CustomQueryArgs args)
    {
        args.EnsureParent("Artist");
        return (IRavenQueryable<Song>)session.Query<Song>()
            .Where(s => !s.IsDeleted && s.Artists.Any(a => a.ArtistId == args.Parent!.Id));
    }
}
