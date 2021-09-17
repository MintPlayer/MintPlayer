using System.Threading.Tasks;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Parsers;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Parsers
{
	internal class ArtistV1Parser : IArtistV1Parser
	{
		public Task<MintPlayer.Fetcher.Abstractions.Dtos.Artist> Parse(string html, string pageData)
		{
			throw new System.NotImplementedException();
		}
	}
}
