using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius.Parsers
{
	internal interface IGeniusParser
	{
		bool IsMatch(string url, string html);
		Task<Dtos.Subject> Parse(string url, string html, bool trimTrash);
	}
}
