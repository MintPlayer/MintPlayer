using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Data
{
	internal class LyricsBody
	{
		[JsonProperty("html")]
		public string Html { get; set; }
	}
}
