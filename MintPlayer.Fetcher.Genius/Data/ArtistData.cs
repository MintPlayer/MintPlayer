using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MintPlayer.Fetcher.Genius.Data
{
    internal class ArtistData
    {
        [JsonProperty("artist")]
        public Artist Artist { get; set; }

        [JsonProperty("artist_albums")]
        public List<Album> Albums { get; set; }

        [JsonProperty("artist_songs")]
        public List<Song> Songs { get; set; }

        public Dtos.Artist ToDto()
        {
            var artist = Artist.ToDto();
            artist.Songs = Songs.Select(s => s.ToDto()).ToList();
            artist.Albums = Albums.Select(a => a.ToDto()).ToList();
            return artist;
        }
    }
}
