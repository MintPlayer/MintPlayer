using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Shared
{
    internal class Artist : Subject
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }
    }
}
