using Newtonsoft.Json;
using System;
using System.Linq;

namespace MintPlayer.Fetcher.Genius.Parsers.V3.Song
{
    internal class SongPageData
    {
        [JsonProperty("song")]
        public Shared.Song Song { get; set; }

        [JsonProperty("lyrics_data")]
        public LyricsData LyricsData { get; set; }

        public Dtos.Song ToDto()
        {
            var media = new System.Collections.Generic.List<Dtos.Medium>();
            if (!string.IsNullOrEmpty(Song.YoutubeUrl))
            {
                media.Add(new Dtos.Medium
                {
                    Type = Enums.eMediumType.YouTube,
                    Value = Song.YoutubeUrl
                });
            }
            if (!string.IsNullOrEmpty(Song.SoundCloudUrl))
            {
                media.Add(new Dtos.Medium
                {
                    Type = Enums.eMediumType.SoundCloud,
                    Value = Song.SoundCloudUrl
                });
            }
            if (!string.IsNullOrEmpty(Song.SpotifyUuid))
            {
                media.Add(new Dtos.Medium
                {
                    Type = Enums.eMediumType.Spotify,
                    Value = Song.SpotifyUuid
                });
            }
            if (!string.IsNullOrEmpty(Song.AppleMusicId))
            {
                media.Add(new Dtos.Medium
                {
                    Type = Enums.eMediumType.Apple,
                    Value = Song.AppleMusicId
                });
            }


            return new Dtos.Song
            {
                Id = Song.Id,
                Url = Song.Url,
                Title = Song.Title,
                ReleaseDate = Song.ReleaseDate,
                PrimaryArtist = new Dtos.Artist
                {
                    Id = Song.PrimaryArtist.Id,
                    Name = Song.PrimaryArtist.Name,
                    Url = Song.PrimaryArtist.Url
                },
                FeaturedArtists = Song.FeaturedArtists.Select(a => new Dtos.Artist
                {
                    Id = a.Id,
                    Name = a.Name,
                    Url = a.Url
                }).ToList(),
                Lyrics = LyricsData.Body.Html,
                Media = media
            };
        }
    }
}
