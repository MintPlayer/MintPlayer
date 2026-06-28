using MintPlayer.Domain.Entities;
using MintPlayer.Spark.Services;

namespace MintPlayer.Web.Actions;

/// <summary>
/// Actions for <see cref="MediumType"/>. Auto-discovered and DI-registered by the framework's
/// actions generator (it resolves the <c>{Entity}Actions</c> naming convention through the
/// <see cref="EntityActions{T}"/> base). Inherits the shared audit + soft-delete behaviour;
/// no MediumType-specific business logic yet, so the body stays empty.
/// </summary>
public class MediumTypeActions : EntityActions<MediumType>
{
    public MediumTypeActions(IEntityMapper entityMapper) : base(entityMapper) { }
}
