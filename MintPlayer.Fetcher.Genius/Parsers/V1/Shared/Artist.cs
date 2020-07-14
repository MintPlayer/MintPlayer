using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Shared
{
    internal class Artist
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        public Dtos.Artist ToDto()
        {
            return new Dtos.Artist
            {
                Id = Id,
                Name = Name,
                ImageUrl = ImageUrl,
                Url = Url
            };
        }
    }
}
