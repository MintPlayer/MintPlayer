using System.Threading.Tasks;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Artist
{
	internal interface IArtistV1Parser
	{
		Task<Subject> Parse(string html);
	}

	internal class ArtistV1Parser : IArtistV1Parser
	{
		public Task<Subject> Parse(string html)
		{
			throw new System.NotImplementedException();
		}
	}
}
