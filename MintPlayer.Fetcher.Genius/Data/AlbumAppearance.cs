using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Data
{
    internal class AlbumAppearance
    {
        [JsonProperty("track_number")]
        public int? TrackNumber { get; set; }

        [JsonProperty("song")]
        public Song Song { get; set; }
    }
}
