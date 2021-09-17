using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;

namespace MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Parsers
{
	public interface IAlbumV1Parser
	{
		Task<Album> Parse(string html, string pageData);
	}
}
