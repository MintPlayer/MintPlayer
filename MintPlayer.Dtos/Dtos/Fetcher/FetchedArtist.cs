using System;
using System.Collections.Generic;
using System.Text;

namespace MintPlayer.Dtos.Dtos.Fetcher
{
    public class FetchedArtist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }

        public List<Fetched<Song>> Songs { get; set; }
        public List<Fetched<Album>> Albums { get; set; }
    }
}
