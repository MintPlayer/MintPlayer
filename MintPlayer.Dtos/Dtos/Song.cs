using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MintPlayer.Dtos.Dtos
{
    public class Song : Subject
    {
        public string Title { get; set; }
        public DateTime Released { get; set; }
        public Lyrics Lyrics { get; set; }

        public string YoutubeId { get; internal set; }
        public string DailymotionId { get; internal set; }
        public PlayerInfo PlayerInfo { get; internal set; }
        public string Description { get; internal set; }

        public List<Artist> Artists { get; set; }

        [JsonIgnore]
        public CompletionField TitleSuggest => new CompletionField { Input = new[] { Title } };
    }
}
