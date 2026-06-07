using MintPlayer.Domain.Entities;
using MintPlayer.Spark.Services;

namespace MintPlayer.Web.Actions;

/// <summary>
/// Actions for <see cref="Tag"/>. Inherits the shared audit + soft-delete behaviour; no
/// tag-specific business logic yet.
/// </summary>
public class TagActions : EntityActions<Tag>
{
    public TagActions(IEntityMapper entityMapper) : base(entityMapper) { }
}
