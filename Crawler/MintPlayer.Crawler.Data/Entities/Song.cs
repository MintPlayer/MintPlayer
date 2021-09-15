using System;
using System.Collections.Generic;
using System.Text;

namespace MintPlayer.Crawler.Data.Entities
{
    internal class Song
    {
        public int Id { get; set; }
        public Subject Subject { get; set; }

        public List<Artist> Artists { get; set; }
        public Album Album { get; set; }
    }
}
