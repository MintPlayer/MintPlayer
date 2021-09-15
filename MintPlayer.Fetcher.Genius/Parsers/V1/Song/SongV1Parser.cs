using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.Genius.Services;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Song
{
	internal interface ISongV1Parser
	{
		Task<Subject> Parse(string html, string pageData);
	}

	internal class SongV1Parser : ISongV1Parser
	{
		private readonly IPageDataReader pageDataReader;
		public SongV1Parser(IPageDataReader pageDataReader)
		{
			this.pageDataReader = pageDataReader;
		}

		public async Task<Subject> Parse(string html, string pageData)
		{
			throw new System.NotImplementedException();
		}
	}
}
