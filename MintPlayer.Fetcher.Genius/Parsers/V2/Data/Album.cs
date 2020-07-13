using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Data
{
    internal class Album : Subject
    {
        [JsonProperty("byArtist")]
        public Artist ByArtist { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("numTracks")]
        public int NumTracks { get; set; }
    }
}
