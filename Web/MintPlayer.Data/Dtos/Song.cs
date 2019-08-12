using Nest;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MintPlayer.Data.Dtos
{
    public class Song : Subject
    {
        public string Title { get; set; }
        public DateTime Released { get; set; }
        public string Lyrics { get; set; }

        public string Text => Title;

        [JsonIgnore]
        [Completion]
        public CompletionField TitleSuggest => new CompletionField { Input = new[] { Title } };

        public List<Artist> Artists { get; set; }
    }
}
