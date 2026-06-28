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

        public override string[] RelatedUrls
        {
            get
            {
                var result = new List<string>();
                if (Songs != null)
                    result.AddRange(Songs.Select(s => s.Url));
                if (Albums != null)
                    result.AddRange(Albums.Select(t => t.Url));
                return result.ToArray();
            }
        }
    }
}
