using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Musixmatch.Data
{
    internal class Song
    {
        [JsonProperty("name")]
        public string Title { get; set; }

        public Dtos.Song ToDto()
        {
            return new Dtos.Song
            {
                Title = Title
            };
        }
    }
}
