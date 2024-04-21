using Newtonsoft.Json.Converters;

namespace MintPlayer.Fetcher.Converters;

public class DateFormatConverter : IsoDateTimeConverter
{
	public DateFormatConverter(string format)
	{
		DateTimeFormat = format;
	}
}
