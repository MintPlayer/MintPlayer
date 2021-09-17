using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Data
{
	internal class MediumData
	{
		[JsonProperty("provider")]
		public string Provider { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }
	}
}
