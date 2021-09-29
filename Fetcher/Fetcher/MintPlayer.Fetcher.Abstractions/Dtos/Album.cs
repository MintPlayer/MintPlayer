using System;
using System.Collections.Generic;
using System.Linq;

namespace MintPlayer.Fetcher.Abstractions.Dtos
{
    public class Album : Subject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public Artist Artist { get; set; }
        public string CoverArtUrl { get; set; }
        public string Url { get; set; }
        public List<Song> Tracks { get; set; }

        public override SubjectLookup[] RelatedUrls
        {
            get
            {
                var result = new List<SubjectLookup>();
                if (Artist != null)
                    result.Add(new SubjectLookup { Url = Artist.Url, Keyword = Artist.Name, SubjectTypes = new string[] { "artist" } });
                if (Tracks != null)
                    result.AddRange(Tracks.Select(t => new SubjectLookup { Url = t.Url, Keyword = t.Title, SubjectTypes = new string[] { "song" } }));
                return result.ToArray();
            }
        }
    }
}
