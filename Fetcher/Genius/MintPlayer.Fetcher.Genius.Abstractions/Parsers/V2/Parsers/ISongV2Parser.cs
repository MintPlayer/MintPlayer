using MintPlayer.Fetcher.Abstractions.Dtos;

namespace MintPlayer.Fetcher.Genius.Abstractions.Parsers.V2.Parsers;

public interface ISongV2Parser
{
	Task<Song> Parse(string html, string preloadedState, bool trimTrash);
}
