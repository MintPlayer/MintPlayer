using MintPlayer.Fetcher.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius.Parsers.V2
{
    internal interface ISongParser
    {
        Task<Subject> ParseSong(string html, string ldJson);
    }
    internal class SongParser : ISongParser
    {
        public async Task<Subject> ParseSong(string html, string ldJson)
        {
            var song = JsonConvert.DeserializeObject<Data.Song>(ldJson);
            //return new Song
            //{
                
            //}
            throw new NotImplementedException();
        }

        private List<Data.Artist> ExtractFeaturedArtists(string html)
        {
            throw new NotImplementedException();
        }

        private string ExtractLyrics(string html, bool trimTrash)
        {
            var lyricsRegex = new Regex(@"(?<=\<div class\=\""lyrics\""\>).*?(?=\<\/div\>)", RegexOptions.Singleline | RegexOptions.Multiline);
            var lyricsMatch = lyricsRegex.Match(html);
            if (!lyricsMatch.Success) throw new Exception("No lyrics tag found");

            var stripARegex = new Regex(@"\<a.*?\>|\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var stripped = stripARegex.Replace(lyricsMatch.Value, "");
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
