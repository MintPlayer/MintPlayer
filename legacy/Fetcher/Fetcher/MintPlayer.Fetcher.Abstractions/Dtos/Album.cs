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

        public override string[] RelatedUrls
        {
            get
            {
                var result = new List<string>();
                if (Artist != null)
                    result.Add(Artist.Url);
                if (Tracks != null)
                    result.AddRange(Tracks.Select(t => t.Url));
                return result.ToArray();
            }
        }
    }
}
