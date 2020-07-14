using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V3.Shared
{
    internal class Medium
    {
        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("type")]
        public MediumType Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
