using System.Threading.Tasks;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Artist;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Artist
{
	internal class ArtistV1Parser : IArtistV1Parser
	{
		public Task<MintPlayer.Fetcher.Abstractions.Dtos.Artist> Parse(string html, string pageData)
		{
			throw new System.NotImplementedException();
		}
	}
}
