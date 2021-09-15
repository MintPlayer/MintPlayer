using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Services
{
	internal interface IPreloadedStateReader
	{
		Task<string> ReadPreloadedState(string html);
	}

	internal class PreloadedStateReader : IPreloadedStateReader
	{
		public Task<string> ReadPreloadedState(string html)
		{
			var rgx = new Regex(@"window\.__PRELOADED_STATE__ \= JSON\.parse\(\'(?<preloadedstate>.*)\'\)\;$");
			var match = rgx.Match(html);
			if (match.Success)
			{
				var preloadedState = match.Groups["preloadedstate"].Value;
				return Task.FromResult(Regex.Unescape(preloadedState));
			}
			else
			{
				throw new Exception("No preloaded_state tag found");
			}
		}
	}
}
