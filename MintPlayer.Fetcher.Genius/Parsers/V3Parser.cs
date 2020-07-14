using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.Genius.Parsers.V3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MintPlayer.Fetcher.Genius.Parsers
{
    internal interface IV3Parser
    {
        Task<Subject> Parse(string html, bool trimTrash);
    }
    internal class V3Parser : IV3Parser
    {
        public V3Parser()
        {
        }

        public async Task<Subject> Parse(string html, bool trimTrash)
        {
            var rgx = new Regex(@"\<meta content\=\""(?<data>.*?)\"" itemprop\=\""page_data\""\>\<\/meta\>", RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (m.Success)
            {
                var information = HttpUtility.HtmlDecode(m.Groups["data"].Value);
                var data = JsonConvert.DeserializeObject<SongPageData>(information);
                throw new NotImplementedException();
            }
            else
            {
                throw new Exception("No pagedata tag found");
            }
        }
    }
}
