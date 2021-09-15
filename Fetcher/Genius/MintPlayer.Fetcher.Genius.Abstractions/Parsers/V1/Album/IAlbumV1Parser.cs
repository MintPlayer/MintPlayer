using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;

namespace MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Album
{
	public interface IAlbumV1Parser
	{
		Task<Fetcher.Abstractions.Dtos.Album> Parse(string html, string pageData);
	}
}
