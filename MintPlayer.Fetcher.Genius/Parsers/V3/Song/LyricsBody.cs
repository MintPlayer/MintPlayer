using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V3.Song
{
    internal class LyricsBody
    {
        [JsonProperty("html")]
        public string Html { get; set; }
    }
}
