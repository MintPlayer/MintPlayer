using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Parsers;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Parsers
{
	internal class SongV1Parser : ISongV1Parser
	{
		private readonly Mappers.SongV1Mapper songMapper;
		public SongV1Parser(Mappers.SongV1Mapper songMapper)
		{
			this.songMapper = songMapper;
		}

		public async Task<MintPlayer.Fetcher.Abstractions.Dtos.Song> Parse(string html, string pageData, bool trimTrash)
		{
			var songPageData = JsonConvert.DeserializeObject<PageData.SongPageData>(pageData);
			var song = await songMapper.ToDto(songPageData.Song, songPageData.LyricsData, trimTrash);
			return song;
		}
	}
}
