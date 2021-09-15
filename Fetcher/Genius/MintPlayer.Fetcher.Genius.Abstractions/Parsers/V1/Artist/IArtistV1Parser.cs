using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;

namespace MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Artist
{
	public interface IArtistV1Parser
	{
		Task<Fetcher.Abstractions.Dtos.Artist> Parse(string html, string pageData);
	}
}
