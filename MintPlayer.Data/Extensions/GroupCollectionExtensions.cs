using System.Text.RegularExpressions;

namespace MintPlayer.Data.Extensions;

public static class GroupCollectionExtensions
{
	public static IEnumerable<Group> Where(this GroupCollection collection, Func<Group, bool> predicate)
	{
		var data = new Group[collection.Count];
		collection.CopyTo(data, 0);
		return data.Where(predicate);
	}

	public static Group? FirstOrDefault(this GroupCollection collection, Func<Group, bool> predicate)
	{
		var data = new Group[collection.Count];
		collection.CopyTo(data, 0);
		return data.FirstOrDefault(predicate);
	}
}
