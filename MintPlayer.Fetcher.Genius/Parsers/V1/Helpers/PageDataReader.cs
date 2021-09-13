using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Helpers
{
    internal interface IPageDataReader
    {
        Task<string> Read(string html);
    }
    internal class PageDataReader : IPageDataReader
    {
        public Task<string> Read(string html)
        {
            var pageDataRegex = new Regex(@"window\.__PRELOADED_STATE__\s\=\sJSON\.parse\(\'(?<data>.*?)\'\)\;");

            var rawPageData =
                pageDataRegex.Match(html).Groups["data"].Value
                .Replace(@"\""", @"""")
                .Replace(@"\\", @"\")
				.Replace(@"\$", @"$");

            var decodedPageData = HttpUtility.HtmlDecode(rawPageData);

            return Task.FromResult(decodedPageData);
        }
    }
}
