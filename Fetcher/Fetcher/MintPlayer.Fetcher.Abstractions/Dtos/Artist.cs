using System.Collections.Generic;
using System.Linq;

namespace MintPlayer.Fetcher.Abstractions.Dtos
{
    public class Artist : Subject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }

        public List<Song> Songs { get; set; }
        public List<Album> Albums { get; set; }

        public override SubjectLookup[] RelatedUrls
        {
            get
            {
                var result = new List<SubjectLookup>();
                if (Songs != null)
                    result.AddRange(Songs.Select(t => new SubjectLookup { Url = t.Url, Keyword = t.Title, SubjectTypes = new string[] { "song" } }));
                if (Albums != null)
                    result.AddRange(Albums.Select(t => new SubjectLookup { Url = t.Url, Keyword = t.Name, SubjectTypes = new string[] { "album" } }));
                return result.ToArray();
            }
        }
    }
}
