using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V3
{
    internal class SongPageData
    {
        [JsonProperty("song")]
        public Song Song { get; set; }

        [JsonProperty("lyrics_data")]
        public LyricsData LyricsData { get; set; }
    }
}
