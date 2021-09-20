using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.Helpers;

namespace MintPlayer.Fetcher.Genius.Parsers.Helpers
{
	internal class LdJsonReader : ILdJsonReader
	{
		public Task<string> ReadLdJson(string html)
		{
			var rgx = new Regex(@"\<script type\=""application\/ld\+json\""\>\s*(?<json>.*?)\s*\<\/script\>", RegexOptions.Multiline);
			var m = rgx.Match(html);
			if (!m.Success)
			{
				throw new Exception("No LD+json tag found");
			}

			var json = m.Groups["json"].Value;
			return Task.FromResult(json);
		}
	}
}
