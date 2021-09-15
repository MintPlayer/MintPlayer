using System;
using System.Collections.Generic;
using System.Text;

namespace MintPlayer.Fetcher.SongtekstenNet.Data
{
    internal class Artist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public Dtos.Artist ToDto()
        {
            return new Dtos.Artist
            {
                Id = Id,
                Name = Name,
                Url = Url
            };
        }
    }
}
