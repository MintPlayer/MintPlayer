using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Abstractions
{
	public interface IFetcherContainer
	{
		IFetcher GetFetcher(string url);
		Task<Dtos.Subject> Fetch(string url, bool trimTrash);
	}
}
