using System;
using System.Collections.Generic;
using System.Text;

namespace MintPlayer.Crawler.Data.Entities
{
    internal class Artist
    {
        public int Id { get; set; }
        public Subject Subject { get; set; }

        public List<Song> Songs { get; set; }
        public List<Album> Albums { get; set; }
    }
}
