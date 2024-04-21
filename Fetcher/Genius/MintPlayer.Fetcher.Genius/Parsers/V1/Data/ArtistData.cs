using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Data;

internal class ArtistData
{
	[JsonProperty("id")]
	public long Id { get; set; }

	[JsonProperty("url")]
	public string Url { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }
}
