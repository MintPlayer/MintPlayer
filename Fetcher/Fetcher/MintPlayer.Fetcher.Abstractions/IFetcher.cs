using System.Text.RegularExpressions;

namespace MintPlayer.Fetcher.Abstractions;

public interface IFetcher
{
	IEnumerable<Regex> UrlRegex { get; }
	Task<Dtos.Subject> Fetch(string url, bool trimTrash);
}
