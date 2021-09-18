using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.Helpers;

namespace MintPlayer.Fetcher.Genius.Parsers.Helpers
{
	internal class LyricsParser : ILyricsParser
	{
		public Task<string> ParseLyrics(string lyricsHtml, bool trimTrash)
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
