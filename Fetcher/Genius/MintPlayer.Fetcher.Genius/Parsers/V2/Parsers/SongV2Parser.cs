using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V2.Parsers;
using MintPlayer.Fetcher.Genius.Parsers.V2.Mappers;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Parsers
{
	internal class SongV2Parser : ISongV2Parser
	{
		private readonly SongV2Mapper songV2Mapper;
		public SongV2Parser(SongV2Mapper songV2Mapper)
		{
			this.songV2Mapper = songV2Mapper;
		}

		public async Task<Song> Parse(string html, string preloadedState)
		{
			try
			{
				var songPreloadedState = JsonConvert.DeserializeObject<Common.SongPreloadedState>(preloadedState);
				var song = await songV2Mapper.ToDto(songPreloadedState);
				return song;
			}
			catch (System.Exception ex)
			{
				throw;
			}
		}
	}
}
