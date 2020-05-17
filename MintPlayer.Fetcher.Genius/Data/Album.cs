using MintPlayer.Fetcher.Converters;
using Newtonsoft.Json;
using System;

namespace MintPlayer.Fetcher.Genius.Data
{
    internal class Album
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("release_date")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd")]
        public DateTime ReleaseDate { get; set; }

        [JsonProperty("artist")]
        public Artist Artist { get; set; }

        [JsonProperty("cover_art_url")]
        public string CoverArtUrl { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        public Dtos.Album ToDto()
        {
            return new Dtos.Album
            {
                Id = Id,
                Name = Name,
                ReleaseDate = ReleaseDate,
                CoverArtUrl = CoverArtUrl,
                Artist = Artist.ToDto(),
                Url = Url
            };
        }
    }
}
