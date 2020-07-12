using MintPlayer.Fetcher.Dtos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.LyricsCom.Parsers
{
    internal interface ISongParser
    {
        Task<Subject> ParseSong(string url, string html);
    }

    internal class SongParser : ISongParser
    {
        public Task<Subject> ParseSong(string url, string html)
        {
            var artists = ExtractSongArtists(html);

            var result = new Song
            {
                Id = ExtractSongId(url),
                Url = url,
                Title = ExtractSongTitle(html),
                PrimaryArtist = artists.FirstOrDefault(),
                FeaturedArtists = artists.Skip(1).ToList(),
                Lyrics = ExtractSongLyrics(html, true),
                Media = new List<Medium>()
            };

            var youtube = ExtractSongYoutubeUrl(html);
            if (youtube != null)
                result.Media.Add(new Medium { Type = Enums.eMediumType.YouTube, Value = youtube });

            return Task.FromResult<Subject>(result);
        }

        private int ExtractSongId(string url)
        {
            var parts = url.Split('/');

            int result;
            var success = int.TryParse(parts[4], out result);
            if (!success) Debugger.Break();
            return result;
        }

        private string ExtractSongTitle(string html)
        {
            var h1Regex = new Regex(@"\<h1.*?\>(?<title>.*?)\<\/h1\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var h1Match = h1Regex.Match(html);
            if (!h1Match.Success) throw new Exception("No H1 tag found");

            return h1Match.Groups["title"].Value;
        }

        private string ExtractSongLyrics(string html, bool trimTrash)
        {
            var preRegex = new Regex(@"\<pre.*?\>(?<body>.*?)\<\/pre\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var preMatch = preRegex.Match(html);
            if (!preMatch.Success) throw new Exception("No pre tag found");

            var stripARegex = new Regex(@"\<a.*?\>|\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var stripped = stripARegex.Replace(preMatch.Groups["body"].Value, "");
            var whitespaces_stripped = stripped.Replace("\r\n", Environment.NewLine).Trim();

            return whitespaces_stripped;
        }

        private string ExtractSongYoutubeUrl(string html)
        {
            var idRegex = new Regex(@"\<div class=\""youtube-player\"" data-id=\""(?<id>.*?)\""\>\<\/div\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var idMatch = idRegex.Match(html);
            if (!idMatch.Success) return null;

            var id = idMatch.Groups["id"].Value;

            return $"http://www.youtube.com/watch?v={id}";
        }

        private IEnumerable<Artist> ExtractSongArtists(string html)
        {
            var h3Regex = new Regex(@"\<h3 class=\""lyric-artist\""\>(?<h3>.*?)\<\/h3\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var h3Match = h3Regex.Match(html);
            if (!h3Match.Success) return null;

            var aRegex = new Regex(@"\<a href=\""(?<url>artist\/.*?)\""\>(?<name>.*?)\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var aMatches = aRegex.Matches(h3Match.Groups["h3"].Value);

            var aMatchesArray = new Match[aMatches.Count];
            aMatches.CopyTo(aMatchesArray, 0);

            return aMatchesArray.Select(m => new Artist
            {
                Name = m.Groups["name"].Value,
                Url = "https://www.lyrics.com/" + m.Groups["url"].Value
            });
        }
    }
}
