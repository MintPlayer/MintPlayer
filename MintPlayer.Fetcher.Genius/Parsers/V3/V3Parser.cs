using MintPlayer.Fetcher.Dtos;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MintPlayer.Fetcher.Genius.Parsers.V3
{
    internal class V3Parser : IGeniusVersionParser
    {
        public V3Parser()
        {
        }

        public Task<bool> IsMatch(string html)
        {
            var isMatch = html.Contains(@"itemprop=""page_data""");
            return Task.FromResult(isMatch);
        }

        public Task<Subject> Parse(string html, bool trimTrash)
        {
            var rgx = new Regex(@"\<meta content\=\""(?<data>.*?)\"" itemprop\=\""page_data\""\>\<\/meta\>", RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (m.Success)
            {
                var information = HttpUtility.HtmlDecode(m.Groups["data"].Value);
                var data = JsonConvert.DeserializeObject<Song.SongPageData>(information);
                var result = data.ToDto();

                result.Lyrics = ExtractLyrics(result.Lyrics, true);
                return Task.FromResult<Subject>(result);
            }
            else
            {
                throw new Exception("No pagedata tag found");
            }
        }

        private string ExtractLyrics(string html, bool trimTrash)
        {
            var lyricsRegex = new Regex(@"(?<=\<div class\=\""lyrics\""\>).*?(?=\<\/div\>)", RegexOptions.Singleline | RegexOptions.Multiline);
            var lyricsMatch = lyricsRegex.Match(html);
            if (!lyricsMatch.Success) throw new Exception("No lyrics tag found");

            var stripPregex = new Regex(@"\<p\>(?<lyrics>.*?)\<\/p\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var stripPMatch = stripPregex.Match(lyricsMatch.Value);

            var whitespaces_stripped = StripTags(stripPMatch.Groups["lyrics"].Value);
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
