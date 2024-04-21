using System.Text.RegularExpressions;

namespace MintPlayer.Fetcher;

public abstract class Fetcher : Abstractions.IFetcher
{
	public abstract IEnumerable<Regex> UrlRegex { get; }
	public abstract Task<Abstractions.Dtos.Subject> Fetch(string url, bool trimTrash);

	protected async Task<string> SendRequest(HttpClient httpClient, string url)
	{
		var response = await httpClient.GetAsync(url);
		var html = await response.Content.ReadAsStringAsync();
		return html;
	}
}
