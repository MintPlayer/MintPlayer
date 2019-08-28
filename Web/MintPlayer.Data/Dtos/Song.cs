using Nest;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MintPlayer.Data.Dtos
{
    public class Song : Subject
    {
        public string Title { get; set; }
        public DateTime Released { get; set; }
        public string Lyrics { get; set; }

        public override string Text => Title;
        public string YoutubeId
        {
            get
            {
                if (Media == null) return null;

                var youtubeVideo = Media.FirstOrDefault(m => m.Type.PlayerType == Enums.ePlayerType.Youtube);
                if (youtubeVideo == null) return null;

                var m1 = Regex.Match(youtubeVideo.Value, "http[s]{0,1}://youtu.be/(.+)$");
                if (m1.Success) return m1.Value;

                var m2 = Regex.Match(youtubeVideo.Value, @"http[s]{0,1}://www.youtube.com/watch\?(.+)$");
                if (!m2.Success) return null;

                var query = m2.Groups.FirstOrDefault(g => g.GetType() != typeof(Match)).Value;
                var parameters = query.Split('&').ToDictionary((item) => item.Split('=', 2).ElementAt(0), (item) => item.Split('=', 2).ElementAt(1));
                if (parameters.ContainsKey("v")) return parameters["v"];
                else return null;
            }
        }
        public string Description
        {
            get
            {
                if (Artists == null)
                    return Title;
                else
                    return Title + " - " + string.Join(" & ", Artists.Select(a => a.Name));
            }
        }

        [JsonIgnore]
        [Completion]
        public CompletionField TitleSuggest => new CompletionField { Input = new[] { Title } };

        public List<Artist> Artists { get; set; }
    }
}
