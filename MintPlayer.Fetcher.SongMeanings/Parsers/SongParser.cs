using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.SongMeanings.Parsers
{
    internal interface ISongParser
    {
        Task<Subject> ParseSong(string url, string html);
    }

    internal class SongParser : ISongParser
    {
        public Task<Subject> ParseSong(string url, string html)
        {
            var artistName = ExtractSongArtist(html);
            var song = new Song
            {
                Url = url,
                Title = ExtractSongTitle(html, artistName.Name),
                Lyrics = ExtractSongLyrics(html),
                PrimaryArtist = artistName,
                FeaturedArtists = null
            };
            return Task.FromResult<Subject>(song);
        }

        private string ExtractSongTitle(string html, string artistName)
        {
            var rgx = new Regex($@"\<title\>{artistName} \- (?<title>.+) Lyrics \| SongMeanings\<\/title\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            var val = m.Groups["title"].Value;

            var rgxFeat = new Regex(@"(?<real_title>.+) \(feat\. .+\)", RegexOptions.Singleline);
            var mFeat = rgxFeat.Match(val);
            if (mFeat.Success)
            {
                return mFeat.Groups["real_title"].Value;
            }
            else
            {
                return val;
            }
        }
        private string ExtractSongLyrics(string html)
        {
            var rgx = new Regex(@"\<div class\=\""holder lyric\-box\""\>\s*(?<lyrics>.*?)\<div\s", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            var lyrics = m.Groups["lyrics"].Value;
            return lyrics.Replace("<br>", string.Empty).Replace("\n", Environment.NewLine);
        }
        private Artist ExtractSongArtist(string html)
        {
            var rgx = new Regex(@"\<h3\>More \<a href=\""(?<url>.*?)\"" title=\""(?<artist>.*?) Lyrics\"">\k<artist> Lyrics\<\/a\>\<\/h3\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            return new Artist
            {
                Url = m.Groups["url"].Value,
                Name = m.Groups["artist"].Value
            };
        }
    }
}
