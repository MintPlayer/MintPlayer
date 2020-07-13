using MintPlayer.Fetcher.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Data
{
    internal class Song : Subject
    {
        [JsonProperty("byArtist")]
        public Artist ByArtist { get; set; }

        [JsonProperty("inAlbum")]
        public List<Album> InAlbum { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("datePublished")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd")]
        public DateTime DatePublished { get; set; }

        //public Dtos.Song ToDto()
        //{
        //    var artists = FeaturedArtists == null ? new List<Dtos.Artist>() : FeaturedArtists.Select(a => a.ToDto()).ToList();
        //    artists.Add(PrimaryArtist.ToDto());

        //    return new Dtos.Song
        //    {
        //        Id = Id,
        //        Title = Title,
        //        ReleaseDate = ReleaseDate,
        //        PrimaryArtist = PrimaryArtist.ToDto(),
        //        FeaturedArtists = FeaturedArtists == null ? null : FeaturedArtists.Select(a => a.ToDto()).ToList(),
        //        Url = Url,
        //        Lyrics = Lyrics,
        //        Media = new List<Dtos.Medium>(new[]
        //        {
        //            new Dtos.Medium { Type = Enums.eMediumType.Apple, Value = AppleMusicId },
        //            new Dtos.Medium { Type = Enums.eMediumType.SoundCloud, Value = SoundCloudUrl },
        //            new Dtos.Medium { Type = Enums.eMediumType.Spotify, Value = SpotifyUuid },
        //            new Dtos.Medium { Type = Enums.eMediumType.YouTube, Value = YoutubeUrl }
        //        })
        //    };
        //}
    }
}