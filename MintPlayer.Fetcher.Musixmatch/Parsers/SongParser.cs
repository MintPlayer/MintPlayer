using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.Musixmatch.Parsers
{
    internal interface ISongParser
    {
        Task<Subject> ParseSong(string url, string html, string ldJson);
    }
    internal class SongParser : ISongParser
    {
        public Task<Subject> ParseSong(string url, string html, string ldJson)
        {
            var song = JsonConvert.DeserializeObject<Data.Song>(ldJson);

            var artistRegex = new Regex(@"\<a href=\""(?<url>.*?)\"" class=\""mxm-track-title__artist mxm-track-title__artist-link\""\>(?<name>.*?)\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var artistMatches = artistRegex.Matches(html);
            var artistMatchesArr = new Match[artistMatches.Count];
            artistMatches.CopyTo(artistMatchesArr, 0);

            var result = new Song
            {
                Lyrics = ExtractSongLyrics(html, true),
                Title = song.Title,
                PrimaryArtist = new Artist
                {
                    Name = artistMatchesArr.First().Groups["name"].Value,
                    Url = "https://musixmatch.com" + artistMatchesArr.First().Groups["url"].Value
                },
                FeaturedArtists = artistMatchesArr.Skip(1).Select(m => new Artist
                {
                    Name = m.Groups["name"].Value,
                    Url = "https://musixmatch.com" + m.Groups["url"].Value
                }).ToList(),
                Url = url
            };
            return Task.FromResult<Subject>(result);
        }
        private string ExtractSongLyrics(string html, bool trimTrash)
        {
            var spanRegex = new Regex(@"(?<=\<span class=\""lyrics__content__ok\""\>).*?(?=\<\/span\>)", RegexOptions.Singleline | RegexOptions.Multiline);
            var spanMatches = spanRegex.Matches(html);
            if (spanMatches.Count == 0) throw new Exception("span tag not found");

            var matches = new Match[spanMatches.Count];
            spanMatches.CopyTo(matches, 0);

            return string.Join("\r\n\r\n", matches.Select(m => m.Value));
        }
    }
}
