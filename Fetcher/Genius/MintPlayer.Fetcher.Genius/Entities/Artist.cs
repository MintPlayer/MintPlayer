using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Entities
{
	internal class Artist
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("api_path")]
		public string ApiPath { get; set; }

		[JsonProperty("image_url")]
		public string ImageUrl { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }
	}
}
