using MintPlayer.Fetcher.Genius.Parsers.V1.Data.Shared;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Data;

internal class SongPagingResult
{
	[JsonProperty("meta")]
	public Meta Meta { get; set; }

	[JsonProperty("response")]
	public SongPagingResultResponse Response { get; set; }
}

internal class SongPagingResultResponse
{
	[JsonProperty("next_page")]
	public int? NextPage { get; set; }

	[JsonProperty("songs")]
	public List<Entities.Song> Songs { get; set; }
}
