using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Data
{
	internal class SongData
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("release_date")]
		public DateTime ReleaseDate { get; set; }

		[JsonProperty("primary_artist")]
		public ArtistData PrimaryArtist { get; set; }

		[JsonProperty("featured_artists")]
		public List<ArtistData> FeaturedArtists { get; set; }

		[JsonProperty("media")]
		public List<MediumData> Media { get; set; }
	}
}
