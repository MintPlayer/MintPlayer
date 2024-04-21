using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V2.Services;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Services;

internal class PreloadedStateReader : IPreloadedStateReader
{
	public Task<string> ReadPreloadedState(string html)
	{
		var rgx = new Regex(@"window\.__PRELOADED_STATE__ \= JSON\.parse\(\'(?<preloadedstate>.*)\'\)\;", RegexOptions.Multiline);
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
