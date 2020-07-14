using MintPlayer.Fetcher.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MintPlayer.Fetcher.Genius.Parsers.V3
{
    internal class Song
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("release_date")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd")]
        public DateTime ReleaseDate { get; set; }

        [JsonProperty("primary_artist")]
        public Artist PrimaryArtist { get; set; }

        [JsonProperty("featured_artists")]
        public List<Artist> FeaturedArtists { get; set; }

        [JsonProperty("media")]
        public List<Medium> Media { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("youtube_url")]
        public string YoutubeUrl { get; set; }

        [JsonProperty("apple_music_id")]
        public string AppleMusicId { get; set; }

        [JsonProperty("soundcloud_url")]
        public string SoundCloudUrl { get; set; }

        [JsonProperty("spotify_uuid")]
        public string SpotifyUuid { get; set; }
    }
}
