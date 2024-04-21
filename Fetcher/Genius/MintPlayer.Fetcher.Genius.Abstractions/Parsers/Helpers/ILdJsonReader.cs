namespace MintPlayer.Fetcher.Genius.Abstractions.Parsers.Helpers;

public interface ILdJsonReader
{
	Task<string> ReadLdJson(string html);
}
