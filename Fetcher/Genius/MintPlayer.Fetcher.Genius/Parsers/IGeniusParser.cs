namespace MintPlayer.Fetcher.Genius.Parsers;

internal interface IGeniusParser
{
	bool IsMatch(string url, string html);
	Task<MintPlayer.Fetcher.Abstractions.Dtos.Subject> Parse(string url, string html, bool trimTrash);
}
