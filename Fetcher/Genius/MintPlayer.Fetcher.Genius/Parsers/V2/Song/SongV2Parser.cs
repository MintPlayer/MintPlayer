using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Song
{
	internal interface ISongV2Parser
	{
		Task<Subject> Parse(string html, string preloadedState);
	}

	internal class SongV2Parser : ISongV2Parser
	{
		public async Task<Subject> Parse(string html, string preloadedState)
		{
			var songPreloadedState = JsonConvert.DeserializeObject<Common.SongPreloadedState>(preloadedState);
			return new MintPlayer.Fetcher.Abstractions.Dtos.Song
			{

			};
		}
	}
}
