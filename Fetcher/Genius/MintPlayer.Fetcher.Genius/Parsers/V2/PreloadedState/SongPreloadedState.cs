using MintPlayer.Fetcher.Genius.Parsers.V2.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Common;

internal class SongPreloadedState
{
	[JsonProperty("currentPage")]
	public string CurrentPage { get; set; }

	[JsonProperty("songPage")]
	public SongPage SongPage { get; set; }

	[JsonProperty("entities")]
	public JObject Entities { get; set; }
}
