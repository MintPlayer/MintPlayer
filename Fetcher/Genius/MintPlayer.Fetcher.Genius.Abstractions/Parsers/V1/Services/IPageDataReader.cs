using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Services
{
	public interface IPageDataReader
	{
		Task<string> ReadPageData(string html);
	}
}
