using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Dtos;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Song
{
	internal interface ISongV1Parser
	{
		Task<Subject> Parse(string html, string pageData, bool trimTrash);
	}

	internal class SongV1Parser : ISongV1Parser
	{
		public async Task<Subject> Parse(string html, string pageData, bool trimTrash)
		{
			var songPageData = JsonConvert.DeserializeObject<Common.SongPageData>(pageData);

			return new Dtos.Song
			{
				Lyrics = await ParseLyrics(songPageData.LyricsData.Body.Html, trimTrash)
			};
		}

		private Task<string> ParseLyrics(string lyricsHtml, bool trimTrash)
		{
			var rgx = new Regex(@"\<a [\s\S]*?\>|\<\/a\>|\<p\>|\<\/p\>", RegexOptions.Multiline);
			var stripped = rgx.Replace(lyricsHtml, string.Empty)
				.Replace("\r", string.Empty)
				.Replace("\n", string.Empty)
				.Replace("<br>", Environment.NewLine);

			return Task.FromResult(stripped);
		}
	}
}
