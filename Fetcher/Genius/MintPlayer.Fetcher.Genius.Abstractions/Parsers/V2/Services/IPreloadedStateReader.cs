namespace MintPlayer.Fetcher.Genius.Abstractions.Parsers.V2.Services;

public interface IPreloadedStateReader
{
	Task<string> ReadPreloadedState(string html);
}
