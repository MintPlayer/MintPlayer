namespace MintPlayer.Data.Extensions;

public static class StringExtensions
{
	public static Dictionary<string, string> AsQueryString(this string query)
	{
		var parameters = query.Split('&').ToDictionary((item) => item.Split('=', 2).ElementAt(0), (item) => item.Split('=', 2).ElementAt(1));
		return parameters;
	}
}
