using MintPlayer.Fetcher.Abstractions.Dtos;

namespace MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Parsers;

public interface ISongV1Parser
{
	Task<Song> Parse(string html, string pageData, bool trimTrash);
}
