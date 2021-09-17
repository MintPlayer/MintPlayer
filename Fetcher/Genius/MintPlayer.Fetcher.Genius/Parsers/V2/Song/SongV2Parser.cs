using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V2;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Song
{
	internal class SongV2Parser : ISongV2Parser
	{
		public async Task<MintPlayer.Fetcher.Abstractions.Dtos.Song> Parse(string html, string preloadedState)
		{
			var songPreloadedState = JsonConvert.DeserializeObject<Common.SongPreloadedState>(preloadedState);
			return new MintPlayer.Fetcher.Abstractions.Dtos.Song
			{

			};
		}
	}
}
