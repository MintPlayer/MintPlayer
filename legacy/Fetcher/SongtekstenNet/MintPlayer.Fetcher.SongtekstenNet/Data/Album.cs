using System;
using System.Collections.Generic;
using System.Text;

namespace MintPlayer.Fetcher.SongtekstenNet.Data
{
    internal class Album
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public DateTime Released { get; set; }

        public MintPlayer.Fetcher.Abstractions.Dtos.Album ToDto()
        {
            return new MintPlayer.Fetcher.Abstractions.Dtos.Album
            {
                Id = Id,
                Name = Name,
                ReleaseDate = Released,
                Url = Url
            };
        }
    }
}
