using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Album
{
    internal class AlbumData
    {
        [JsonProperty("album")]
        public Shared.Album Album { get; set; }

        [JsonProperty("album_appearances")]
        public List<AlbumAppearance> Tracks { get; set; }

        public Dtos.Album ToDto()
        {
            return new Dtos.Album
            {
                Id = Album.Id,
                Name = Album.Name,
                ReleaseDate = Album.ReleaseDate,
                CoverArtUrl = Album.CoverArtUrl,
                Artist = Album.Artist.ToDto(),
                Url = Album.Url,
                Tracks = Tracks
                    .OrderBy(t => t.TrackNumber)
                    .Select(t => t.Song.ToDto())
                    .ToList()
            };
        }
    }
}
