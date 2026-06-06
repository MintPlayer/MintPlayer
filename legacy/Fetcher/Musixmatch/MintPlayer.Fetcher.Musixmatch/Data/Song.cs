using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Musixmatch.Data
{
    internal class Song
    {
        [JsonProperty("name")]
        public string Title { get; set; }

        public MintPlayer.Fetcher.Abstractions.Dtos.Song ToDto()
        {
            return new MintPlayer.Fetcher.Abstractions.Dtos.Song
			{
                Title = Title
            };
        }
    }
}
