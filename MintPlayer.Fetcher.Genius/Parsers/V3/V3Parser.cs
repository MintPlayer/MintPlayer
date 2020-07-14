using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.Genius.Helpers;
using MintPlayer.Fetcher.Genius.Parsers.V3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MintPlayer.Fetcher.Genius.Parsers.V3
{
    internal interface IV3Parser
    {
        Task<Subject> Parse(string html, bool trimTrash);
    }
    internal class V3Parser : IV3Parser
    {
        private readonly ILyricsTrimmer lyricsTrimmer;
        public V3Parser(ILyricsTrimmer lyricsTrimmer)
        {
            this.lyricsTrimmer = lyricsTrimmer;
        }

        public async Task<Subject> Parse(string html, bool trimTrash)
        {
            var rgx = new Regex(@"\<meta content\=\""(?<data>.*?)\"" itemprop\=\""page_data\""\>\<\/meta\>", RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (m.Success)
            {
                var information = HttpUtility.HtmlDecode(m.Groups["data"].Value);
                var data = JsonConvert.DeserializeObject<Song.SongPageData>(information);
                var result = data.ToDto();

                result.Lyrics = await lyricsTrimmer.Trim(result.Lyrics, true);
                return result;
            }
            else
            {
                throw new Exception("No pagedata tag found");
            }
        }
    }
}
