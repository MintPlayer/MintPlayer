using System;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.PageData
{
	internal class SongPageData
	{
		[JsonProperty("song")]
		public Data.SongData Song { get; set; }

		[JsonProperty("lyrics_data")]
		public LyricsData LyricsData { get; set; }

		[JsonProperty("primary_artist")]
		public int MyProperty { get; set; }
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
