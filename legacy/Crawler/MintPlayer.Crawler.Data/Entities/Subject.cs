using System;
using System.Collections.Generic;
using System.Text;

namespace MintPlayer.Crawler.Data.Entities
{
    internal class Subject
    {
        public int Id { get; set; }
        public Artist Artist { get; set; }
        public Album Album { get; set; }
        public Song Song { get; set; }
    }
}
