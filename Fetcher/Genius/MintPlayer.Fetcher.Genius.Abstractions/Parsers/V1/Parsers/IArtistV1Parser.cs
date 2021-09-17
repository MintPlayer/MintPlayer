using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;

namespace MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Parsers
{
	public interface IArtistV1Parser
	{
		Task<Artist> Parse(string html, string pageData);
	}
}
