using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius.Abstractions.Parsers.Helpers
{
	public interface ILyricsParser
	{
		Task<string> ParseLyrics(string lyricsHtml, bool trimTrash);
	}
}
