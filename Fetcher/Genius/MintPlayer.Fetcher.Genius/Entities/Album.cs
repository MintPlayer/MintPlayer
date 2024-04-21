using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Entities;

internal class Album
{
	[JsonProperty("id")]
	public long Id { get; set; }

	[JsonProperty("url")]
	public string Url { get; set; }

	[JsonProperty("api_path")]
	public string ApiPath { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("release_date")]
	public DateTime ReleaseDate { get; set; }

	[JsonProperty("cover_art_url")]
	public string CoverArtUrl { get; set; }
}
