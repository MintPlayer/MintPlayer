using MintPlayer.Domain.Entities;
using MintPlayer.Spark.Queries;
using MintPlayer.Spark.Services;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace MintPlayer.Web.Actions;

/// <summary>
/// Actions for <see cref="Tag"/>. Inherits the shared audit + soft-delete behaviour, and exposes
/// parent-scoped child sub-queries shown on detail pages (Spark renders an entity type's
/// <c>persistentObject.queries</c> as sub-grids, passing the current row as the parent):
///  - <see cref="Category_Tags"/> — a TagCategory's tags (shown on the category detail page);
///  - <see cref="Tag_Children"/> — a tag's child tags (shown on the tag detail page, the hierarchy).
/// Both return <see cref="Tag"/>, so they live here (the returned-type's Actions class).
/// </summary>
public class TagActions : EntityActions<Tag>
{
    private readonly IAsyncDocumentSession session;

    public TagActions(IEntityMapper entityMapper, IAsyncDocumentSession session) : base(entityMapper)
    {
        this.session = session;
    }

    /// <summary>Tags belonging to the parent <see cref="TagCategory"/>. Source: "Custom.Category_Tags".</summary>
    public IRavenQueryable<Tag> Category_Tags(CustomQueryArgs args)
    {
        args.EnsureParent("TagCategory");
        return (IRavenQueryable<Tag>)session.Query<Tag>()
            .Where(t => t.CategoryId == args.Parent!.Id && !t.IsDeleted);
    }

    /// <summary>Child tags of the parent <see cref="Tag"/> (self-reference). Source: "Custom.Tag_Children".</summary>
    public IRavenQueryable<Tag> Tag_Children(CustomQueryArgs args)
    {
        args.EnsureParent("Tag");
        return (IRavenQueryable<Tag>)session.Query<Tag>()
            .Where(t => t.ParentId == args.Parent!.Id && !t.IsDeleted);
    }
}
