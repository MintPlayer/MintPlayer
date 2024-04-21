using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Entities;

internal class Song
{
	[JsonProperty("id")]
	public long Id { get; set; }

	[JsonProperty("url")]
	public string Url { get; set; }

	[JsonProperty("api_path")]
	public string ApiPath { get; set; }

	[JsonProperty("title")]
	public string Title { get; set; }

	[JsonProperty("primary_artist")]
	public Artist PrimaryArtist { get; set; }

	[JsonProperty("song_art_image_url")]
	public string ImageUrl { get; set; }
}
