using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Common
{
	internal class PreloadedState
	{
		[JsonProperty("currentPage")]
		public string CurrentPage { get; set; }
	}
}
