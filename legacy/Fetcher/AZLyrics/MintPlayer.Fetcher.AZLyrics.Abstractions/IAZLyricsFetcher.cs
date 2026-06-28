using System;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.AZLyrics.Abstractions
{
	public interface IAZLyricsFetcher
	{
		Task<Fetcher.Abstractions.Dtos.Subject> Fetch(string url, bool trimTrash);
	}
}
