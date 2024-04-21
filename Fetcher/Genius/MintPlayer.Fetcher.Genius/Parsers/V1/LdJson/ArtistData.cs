using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.LdJson;

internal class ArtistData
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("url")]
	public string Url { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("image")]
	public string Image { get; set; }
}
