using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Data
{
    internal class SongData
    {
        [JsonProperty("songPage")]
        public Song Song { get; set; }

        [JsonProperty("lyrics_data")]
        public LyricsData LyricsData { get; set; }
    }

    internal class LyricsData
    {
        public LyricsBody Body { get; set; }
    }

    internal class LyricsBody
    {
        public string Html { get; set; }
    }
}
