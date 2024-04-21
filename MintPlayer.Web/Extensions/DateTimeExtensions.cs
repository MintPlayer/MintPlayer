using System.Globalization;

namespace MintPlayer.Web.Extensions;

public static class DateTimeExtensions
{
	public static string ToISOString(this DateTime dateTime)
		=> dateTime.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture);
}
