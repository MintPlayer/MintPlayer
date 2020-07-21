using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using MintPlayer.Fetcher.Dtos;
using System.Linq;
using System.Web;
using MintPlayer.Fetcher.Genius.Parsers.V1.Helpers;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Song
{
    internal interface ISongParser
    {
        Task<Subject> Parse(string html, bool trimTrash);
    }
    internal class SongParser : ISongParser
    {
        private readonly IPageDataReader pageDataReader;
        public SongParser(IPageDataReader pageDataReader)
        {
            this.pageDataReader = pageDataReader;
        }

        public async Task<Subject> Parse(string html, bool trimTrash)
        {
            var pageData = await pageDataReader.Read(html);
            var data = JsonConvert.DeserializeObject<SongData>(pageData);
            data.Song.Lyrics = ExtractLyrics(html, trimTrash);
            return data.Song.ToDto();
        }

        private string ExtractLyrics(string html, bool trimTrash)
        {
            var rgxLyrics = new Regex(@"\<div class\=\""Lyrics__Container.+?\""\>(?<lyrics>.+?)\<\/div\>", RegexOptions.Multiline | RegexOptions.Singleline);
            var matches = rgxLyrics.Matches(html);
            var list = new Match[matches.Count];
            matches.CopyTo(list, 0);

            var lyrics = string.Join("", list.Select(m => m.Groups["lyrics"].Value).Select(l => l.Contains("<div") ? "<br/>" : l));
            var decoded = HttpUtility.HtmlDecode(lyrics);

            var whitespaces_stripped = StripTags(decoded);
            return trimTrash ? StripBrackets(whitespaces_stripped) : whitespaces_stripped;
        }

        private string StripTags(string html)
        {
            var stripARegex = new Regex(@"\<a.*?\>|\<\/a\>|\<span.*?\>|\<\/span\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var aStripped = stripARegex.Replace(html, string.Empty);

            var stripCommentsRegex = new Regex(@"\<\!--.*?--\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var commentsStripped = stripCommentsRegex.Replace(aStripped, string.Empty);

            var whitespaces_stripped = commentsStripped
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("<br>", Environment.NewLine)
                .Replace("<br/>", Environment.NewLine)
                .Replace("<br />", Environment.NewLine);

            return whitespaces_stripped;
        }

        private string StripBrackets(string lyrics)
        {
            var stripBracketsRegex = new Regex(@"\[.*?\]\r\n", RegexOptions.Singleline | RegexOptions.Multiline);
            var brackets_stripped = stripBracketsRegex.Replace(lyrics, "");
            return brackets_stripped;
        }
    }
}
