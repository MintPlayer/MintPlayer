using System;
using System.Collections.Generic;
using System.Linq;

namespace MintPlayer.Fetcher.Abstractions.Dtos
{
    public class Song : Subject
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public Artist PrimaryArtist { get; set; }
        public List<Artist> FeaturedArtists { get; set; }
        public string Url { get; set; }
        public string Lyrics { get; set; }
        public List<Medium> Media { get; set; }

        public override SubjectLookup[] RelatedUrls
        {
            get
            {
                var result = new List<SubjectLookup>();
                if (PrimaryArtist != null)
					result.Add(new SubjectLookup { Url = PrimaryArtist.Url, Keyword = PrimaryArtist.Name, SubjectTypes = new string[] { "artist" } });
				if (FeaturedArtists != null)
                    result.AddRange(FeaturedArtists.Select(t => new SubjectLookup { Url = t.Url, Keyword = t.Name, SubjectTypes = new string[] { "artist" } }));
                return result.ToArray();
            }
        }
    }
}
