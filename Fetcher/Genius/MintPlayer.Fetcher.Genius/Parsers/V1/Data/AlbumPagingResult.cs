using MintPlayer.Fetcher.Genius.Parsers.V1.Data.Shared;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Data;

internal class AlbumPagingResult
{
	[JsonProperty("meta")]
	public Meta Meta { get; set; }

	[JsonProperty("response")]
	public AlbumPagingResultResponse Response { get; set; }
}

internal class AlbumPagingResultResponse
{
	[JsonProperty("next_page")]
	public int? NextPage { get; set; }

	[JsonProperty("albums")]
	public List<Entities.Album> Albums { get; set; }
}
