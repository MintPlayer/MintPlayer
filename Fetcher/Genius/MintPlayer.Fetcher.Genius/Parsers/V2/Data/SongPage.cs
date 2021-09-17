using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Data
{
	internal class SongPage
	{
		[JsonProperty("song")]
		public long Id { get; set; }

		[JsonProperty("path")]
		public string Url { get; set; }

		[JsonProperty("trackingData")]
		public List<KeyValuePair> TrackingData { get; set; }
	}
}
