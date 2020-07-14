using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Song
{
    internal class SongData
    {
        [JsonProperty("songPage")]
        public Shared.Song Song { get; set; }

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
        public System.Collections.Generic.List<object> Children { get; set; } // object = [ string, LyricsNode ]
    }

    internal class LyricsNode
    {
        public System.Collections.Generic.List<object> Children { get; set; } // object = [ string, LyricsNode ]
    }
}
