using MintPlayer.Fetcher.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.AZLyrics.Parsers
{
    internal interface ISongParser
    {
        Task<Subject> ParseSong(string url, string html, bool trimTrash);
    }

    internal class SongParser : ISongParser
    {
        public async Task<Subject> ParseSong(string url, string html, bool trimTrash)
        {
            return new Song
            {
                Url = url,
                Title = await ExtractSongTitle(html),
                Lyrics = await ExtractSongLyrics(html, trimTrash),
                PrimaryArtist = await ExtractSongArtist(html),
                FeaturedArtists = await ExtractSongFeatArtists(html)
            };
        }

        private Task<string> ExtractSongTitle(string html)
        {
            var rgx = new Regex(@"\<b\>\""(?<title>.*?)\""\<\/b\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (!m.Success) throw new Exception("Could not extract song title");

            var title = m.Groups["title"].Value;
            return Task.FromResult(title);
        }
        private Task<Artist> ExtractSongArtist(string html)
        {
            var rgx = new Regex(@"\<a itemprop\=\""item\"" href\=\""(?<url>.*?)"">\<span itemprop\=\""name\""\>(?<artist>.*?) Lyrics\<\/span\>");
            var m = rgx.Match(html);
            if (!m.Success) throw new Exception("No artist found");

            return Task.FromResult(new Artist
            {
                Name = m.Groups["artist"].Value,
                Url = m.Groups["url"].Value
            });
        }
        private Task<List<Artist>> ExtractSongFeatArtists(string html)
        {
            var rgx = new Regex(@"\<span class\=\""feat\""\>\(feat\. (?<feat>.*?)\)\<\/span\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (!m.Success) return Task.FromResult(new List<Artist>());

            var strFeat = m.Groups["feat"].Value;
            var artistNames = Regex.Split(strFeat, @"\,\s|\s\&\s", RegexOptions.Singleline);
            return Task.FromResult(artistNames.Select(an => new Artist { Name = an }).ToList());
        }
        private Task<string> ExtractSongLyrics(string html, bool trimTrash)
        {
            var rgx = new Regex(@"\<div\>\s*\<\!--.*?--\>\s*(?<lyrics>.*?)\s*\<\/div\>\s*\<br\>\<br\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (!m.Success) throw new Exception("No lyrics found");

            if (!trimTrash)
                return Task.FromResult(m.Groups["lyrics"].Value);

            var rgxI = new Regex(@"\<i\>.*?\<\/i\>\<br\>\n", RegexOptions.Singleline | RegexOptions.Multiline);
            var trimmed = rgxI.Replace(m.Groups["lyrics"].Value, string.Empty);

            var result = trimmed.Replace("\r", "").Replace("\n", "").Replace("<br>", Environment.NewLine);

            return Task.FromResult(result);
        }
    }
}
