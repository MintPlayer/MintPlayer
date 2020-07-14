using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MintPlayer.Fetcher.Genius.Parsers.V3
{
    internal class LyricsData
    {
        [JsonProperty("body")]
        public LyricsBody Body { get; set; }
    }
}
