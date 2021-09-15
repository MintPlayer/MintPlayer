using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius.Parsers
{
	internal interface IGeniusParser
	{
		Task<bool> IsMatch(string html);
	}
}
