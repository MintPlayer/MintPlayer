using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Musixmatch.Data;

internal class Subject
{
	[JsonProperty("@type")]
	public string Type { get; set; }
}
