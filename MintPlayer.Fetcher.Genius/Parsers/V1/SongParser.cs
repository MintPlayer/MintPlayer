using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.Genius.Parsers.V1
{
    internal interface ISongParser
    {
        Task<Subject> ParseSong(string pageData, bool trimTrash);
    }
    internal class SongParser : ISongParser
    {
        public Task<Subject> ParseSong(string pageData, bool trimTrash)
        {
            var data = JsonConvert.DeserializeObject<Data.SongData>(pageData);
            if(data.LyricsData.Body.Html == null)
            {

            }
            else
            {
                data.Song.Lyrics = ExtractLyrics(data.LyricsData.Body.Html, trimTrash);
            }
            return Task.FromResult<Subject>(data.Song.ToDto());
        }

        private string ExtractLyrics(string pageDataBodyHtml, bool trimTrash)
        {
            var pRegex = new Regex(@"(?<=\<p\>).*?(?=\<\/p\>)", RegexOptions.Singleline | RegexOptions.Multiline);
            var pMatch = pRegex.Match(pageDataBodyHtml);
            if (!pMatch.Success) throw new Exception("No P tag found");

            var stripARegex = new Regex(@"\<a.*?\>|\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var stripped = stripARegex.Replace(pMatch.Value, "");
            var whitespaces_stripped = stripped.Replace("\r", "").Replace("\n", "").Replace("<br>", Environment.NewLine);

            if (trimTrash)
            {
                var stripBracketsRegex = new Regex(@"\[.*?\]\r\n", RegexOptions.Singleline | RegexOptions.Multiline);
                var brackets_stripped = stripBracketsRegex.Replace(whitespaces_stripped, "");
                return brackets_stripped;
            }
            else
            {
                return whitespaces_stripped;
            }
        }
    }
}
