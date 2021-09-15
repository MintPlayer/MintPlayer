using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Common
{
	internal class SongPageData
	{
		[JsonProperty("lyrics_data")]
		public LyricsData LyricsData { get; set; }
	}

	internal class LyricsData
	{
		[JsonProperty("body")]
		public LyricsDataBody Body { get; set; }
	}

	internal class LyricsDataBody
	{
		[JsonProperty("html")]
		public string Html { get; set; }
	}
}
