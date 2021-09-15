using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MintPlayer.Fetcher.Test")]
namespace MintPlayer.Fetcher.Genius.Parsers.V1.Services
{
	internal interface IPageDataReader
	{
		Task<string> ReadPageData(string html);
	}

	internal class PageDataReader : IPageDataReader
	{
		public Task<string> ReadPageData(string html)
		{
			var rgx = new Regex(@"\<meta content\=\""(?<content>.*)\"" itemprop\=\""page_data\""\>\<\/meta\>");
			var match = rgx.Match(html);
			if (match.Success)
			{
				var pageDataText = match.Groups["content"].Value;
				pageDataText = System.Web.HttpUtility.HtmlDecode(pageDataText);
				return Task.FromResult(pageDataText);
			}
			else
			{
				throw new System.Exception("No page_data tag found");
			}
		}
	}
}
