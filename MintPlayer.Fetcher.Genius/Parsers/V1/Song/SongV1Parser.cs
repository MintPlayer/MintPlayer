using System.Threading.Tasks;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Song
{
	internal interface ISongV1Parser
	{
		Task<Subject> Parse(string html);
	}

	internal class SongV1Parser : ISongV1Parser
	{
		public Task<Subject> Parse(string html)
		{
			throw new System.NotImplementedException();
		}
	}
}
