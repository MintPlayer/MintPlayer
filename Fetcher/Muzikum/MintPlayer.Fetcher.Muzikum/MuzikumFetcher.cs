using MintPlayer.Fetcher.Abstractions.Dtos;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("MintPlayer.Fetcher.Muzikum.Test")]
namespace MintPlayer.Fetcher.Muzikum;

public interface IMuzikumFetcher
{
	Task<Subject> Fetch(string url, bool trimTrash);
}

internal class MuzikumFetcher : Fetcher, IMuzikumFetcher
{
	private readonly HttpClient httpClient;
	public MuzikumFetcher(HttpClient httpClient)
	{
		this.httpClient = httpClient;
	}

	public override IEnumerable<Regex> UrlRegex => [new Regex(@"https\:\/\/(www\.){0,1}muzikum.eu\/.+")];

	public override async Task<Subject> Fetch(string url, bool trimTrash)
	{
		var html = await SendRequest(httpClient, url);
		throw new NotImplementedException();
	}

	#region Private methods
	#region ParseSong

	#endregion
	#endregion
}
