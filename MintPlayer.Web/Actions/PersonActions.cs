using MintPlayer.Domain.Entities;
using MintPlayer.Spark.Abstractions;
using MintPlayer.Spark.Services;

namespace MintPlayer.Web.Actions;

/// <summary>
/// Actions for <see cref="Person"/>. Inherits the shared audit + soft-delete behaviour; trims the
/// name parts on save (FullName itself is computed by the People_Overview index into VPerson).
/// </summary>
public class PersonActions : EntityActions<Person>
{
    public PersonActions(IEntityMapper entityMapper) : base(entityMapper) { }

    public override async Task OnBeforeSaveAsync(PersistentObject obj, Person entity)
    {
        await base.OnBeforeSaveAsync(obj, entity); // audit timestamps
        entity.FirstName = entity.FirstName?.Trim() ?? string.Empty;
        entity.LastName = entity.LastName?.Trim() ?? string.Empty;
    }
}
