using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Shared
{
    internal class Subject
    {
        [JsonProperty("@context")]
        public string Context { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

    }
}
