using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Album
{
    internal class AlbumAppearance
    {
        [JsonProperty("track_number")]
        public int? TrackNumber { get; set; }

        [JsonProperty("song")]
        public Shared.Song Song { get; set; }
    }
}
