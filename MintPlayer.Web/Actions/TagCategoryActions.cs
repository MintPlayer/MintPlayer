using MintPlayer.Domain.Entities;
using MintPlayer.Spark.Services;

namespace MintPlayer.Web.Actions;

/// <summary>
/// Actions for <see cref="TagCategory"/>. Inherits the shared audit + soft-delete behaviour; no
/// category-specific business logic yet.
/// </summary>
public class TagCategoryActions : EntityActions<TagCategory>
{
    public TagCategoryActions(IEntityMapper entityMapper) : base(entityMapper) { }
}
