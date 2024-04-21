using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Data;

internal class LyricsData
{
	[JsonProperty("body")]
	public LyricsBody Body { get; set; }
}
