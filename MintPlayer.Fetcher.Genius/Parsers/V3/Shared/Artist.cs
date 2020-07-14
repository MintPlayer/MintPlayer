using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V3.Shared
{
    internal class Artist
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
