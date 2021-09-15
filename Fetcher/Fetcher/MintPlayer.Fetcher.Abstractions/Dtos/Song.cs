using System;
using System.Collections.Generic;
using System.Linq;

namespace MintPlayer.Fetcher.Abstractions.Dtos
{
    public class Song : Subject
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public Artist PrimaryArtist { get; set; }
        public List<Artist> FeaturedArtists { get; set; }
        public string Url { get; set; }
        public string Lyrics { get; set; }
        public List<Medium> Media { get; set; }

        public override IEnumerable<string> RelatedUrls
        {
            get
            {
                var result = new List<string>();
                if (PrimaryArtist != null)
                    result.Add(PrimaryArtist.Url);
                if (FeaturedArtists != null)
                    result.AddRange(FeaturedArtists.Select(t => t.Url));
                return result;
            }
        }
    }
}
