using System;
using System.Collections.Generic;
using MintPlayer.Data.Entities.Interfaces;

namespace MintPlayer.Data.Entities
{
    internal class Song : Subject, ISoftDelete
    {
        public string Title { get; set; }
        public DateTime Released { get; set; }

        public List<ArtistSong> Artists { get; set; }
        public List<Lyrics> Lyrics { get; set; }
    }
}
