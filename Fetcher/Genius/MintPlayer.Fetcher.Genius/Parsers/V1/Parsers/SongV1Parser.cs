using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Parsers;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Parsers
{
	internal class SongV1Parser : ISongV1Parser
	{
		public async Task<MintPlayer.Fetcher.Abstractions.Dtos.Song> Parse(string html, string pageData, bool trimTrash)
		{
			var songPageData = JsonConvert.DeserializeObject<PageData.SongPageData>(pageData);

			return new MintPlayer.Fetcher.Abstractions.Dtos.Song
			{
				Id = songPageData.Song.Id,
				Title = songPageData.Song.Title,
				ReleaseDate = songPageData.Song.ReleaseDate,
				Lyrics = await ParseLyrics(songPageData.LyricsData.Body.Html, trimTrash),
				//PrimaryArtist = 
			};
		}

		private Task<string> ParseLyrics(string lyricsHtml, bool trimTrash)
		{
			var rgx = new Regex(@"\<a [\s\S]*?\>|\<\/a\>|\<p\>|\<\/p\>", RegexOptions.Multiline);
			var stripped = rgx.Replace(lyricsHtml, string.Empty)
				.Replace("\r", string.Empty)
				.Replace("\n", string.Empty)
				.Replace("<br>", Environment.NewLine);

			if (trimTrash)
			{
				var rgxBrackets = new Regex(@"\[.*?\]\r\n", RegexOptions.Multiline);
				stripped = rgxBrackets.Replace(stripped, string.Empty);
			}

			return Task.FromResult(stripped);
		}
	}
}
