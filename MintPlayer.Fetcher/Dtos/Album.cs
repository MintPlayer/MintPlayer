using System;
using System.Collections.Generic;
using System.Linq;

namespace MintPlayer.Fetcher.Dtos
{
    public class Album : Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public Artist Artist { get; set; }
        public string CoverArtUrl { get; set; }
        public string Url { get; set; }
        public List<Song> Tracks { get; set; }

        public override IEnumerable<string> RelatedUrls
        {
            get
            {
                var result = new List<string>();
                if (Artist != null)
                    result.Add(Artist.Url);
                if (Tracks != null)
                    result.AddRange(Tracks.Select(t => t.Url));
                return result;
            }
        }
    }
}
