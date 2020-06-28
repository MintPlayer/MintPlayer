using MintPlayer.Fetcher.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MintPlayer.Fetcher.Genius.Data
{
    internal class Song
    {
        [JsonProperty("song")]
        public int Id { get; set; }

        [JsonProperty("apple_music_id")]
        public string AppleMusicId { get; set; }

        [JsonProperty("soundcloud_url")]
        public string SoundCloudUrl { get; set; }

        [JsonProperty("spotify_uuid")]
        public string SpotifyUuid { get; set; }

        [JsonProperty("youtube_url")]
        public string YoutubeUrl { get; set; }

        [JsonProperty("release_date")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd")]
        public DateTime? ReleaseDate { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("primary_artist")]
        public Artist PrimaryArtist { get; set; }

        [JsonProperty("featured_artists")]
        public List<Artist> FeaturedArtists { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public string Lyrics { get; set; }

        public Dtos.Song ToDto()
        {
            var artists = FeaturedArtists == null ? new List<Dtos.Artist>() : FeaturedArtists.Select(a => a.ToDto()).ToList();
            artists.Add(PrimaryArtist.ToDto());

            return new Dtos.Song
            {
                Id = Id,
                Title = Title,
                ReleaseDate = ReleaseDate,
                PrimaryArtist = PrimaryArtist.ToDto(),
                FeaturedArtists = FeaturedArtists == null ? null : FeaturedArtists.Select(a => a.ToDto()).ToList(),
                Url = Url,
                Lyrics = Lyrics,
                Media = new List<Dtos.Medium>(new[]
                {
                    new Dtos.Medium { Type = Enums.eMediumType.Apple, Value = AppleMusicId },
                    new Dtos.Medium { Type = Enums.eMediumType.SoundCloud, Value = SoundCloudUrl },
                    new Dtos.Medium { Type = Enums.eMediumType.Spotify, Value = SpotifyUuid },
                    new Dtos.Medium { Type = Enums.eMediumType.YouTube, Value = YoutubeUrl }
                })
            };
        }
    }
}
