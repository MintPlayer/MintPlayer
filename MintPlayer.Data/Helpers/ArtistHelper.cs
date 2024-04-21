using MintPlayer.Data.Entities;

namespace MintPlayer.Data.Helpers;

internal class ArtistHelper
{
	internal void CalculateUpdatedMembers(Artist old, MintPlayer.Dtos.Dtos.Artist @new, MintPlayerContext dbContext, out IEnumerable<ArtistPerson> to_add, out IEnumerable<ArtistPerson> to_update, out IEnumerable<ArtistPerson> to_remove)
	{
		// Compute members to remove
		var en_to_remove = old.Members.Where(ap =>
		{
			if (@new.PastMembers.Any(p => p.Id == ap.PersonId)) return false;
			if (@new.CurrentMembers.Any(p => p.Id == ap.PersonId)) return false;
			return true;
		});

		// Compute members to add
		var en_to_add = @new.CurrentMembers
			.Select(p => new { PersonId = p.Id, Active = true })
			.Concat(@new.PastMembers.Select(p => new { PersonId = p.Id, Active = false }))
			.Where(p => !old.Members.Any(ap => ap.PersonId == p.PersonId))
			.Select(p => new ArtistPerson
			{
				Artist = old,
				ArtistId = old.Id,
				Person = dbContext.People.Find(p.PersonId),
				PersonId = p.PersonId,
				Active = p.Active
			});

		// Compute members to update
		var en_to_update = old.Members.Except(en_to_remove);
		foreach (var item in en_to_update)
		{
			if (@new.CurrentMembers.Any(m => m.Id == item.PersonId))
				item.Active = true;
			else if (@new.PastMembers.Any(m => m.Id == item.PersonId))
				item.Active = false;
			else
				throw new Exception("Not supposed to happen");
		}

		// Yield the results already
		// In the calling method the DbSets are changing. Here we have to evaluate the Linq expressions already.
		to_remove = en_to_remove.ToArray();
		to_add = en_to_add.ToArray();
		to_update = en_to_update.ToArray();
	}
}
