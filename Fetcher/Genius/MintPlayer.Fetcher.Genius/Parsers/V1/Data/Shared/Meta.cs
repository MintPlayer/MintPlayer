using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Data.Shared;

internal class Meta
{
	[JsonProperty("status")]
	public int Status { get; set; }
}
