using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.LoloLyrics.Parsers
{
    internal interface ISongParser
    {
        Task<Subject> ParseSong(string url, string html);
    }

    internal class SongParser : ISongParser
    {
        public Task<Subject> ParseSong(string url, string html)
        {
            var h1 = ExtractHeading(html);
            var year = ExtractYear(html);
            var artists = ExtractArtists(h1);
            var youtube = ExtractYoutubeId(html);
            var media = new List<Medium>();

            if (youtube != null)
            {
                media.Add(new Medium
                {
                    Type = Enums.eMediumType.YouTube,
                    Value = $"https://www.youtube.com/watch?v={youtube}"
                });
            }

            var result = new Song
            {
                Url = url,
                Id = ExtractSongId(url),
                Title = ExtractTitle(h1),
                ReleaseDate = year == null ? (DateTime?)null : new DateTime((int)year, 1, 1),
                Lyrics = ExtractSongLyrics(html, true),
                PrimaryArtist = artists.FirstOrDefault(),
                FeaturedArtists = artists.Skip(1).ToList(),
                Media = media
            };
            return Task.FromResult<Subject>(result);
        }

        private int ExtractSongId(string url)
        {
            var rgx = new Regex(@"https\:\/\/www\.lololyrics\.com\/lyrics\/(?<id>[0-9]+)\.html");
            var m = rgx.Match(url);
            if (!m.Success) throw new Exception("Could not extract song id");

            return Convert.ToInt32(m.Groups["id"].Value);
        }
        private string ExtractHeading(string html)
        {
            var rgx = new Regex(@"\<h1.*?\>(?<heading>.*)\<\/h1\>", RegexOptions.Singleline);
            var m = rgx.Match(html);
            if (!m.Success) throw new Exception("No heading tag found");

            var rgxComment = new Regex(@"\<\!--.*--\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var cleanH1 = rgxComment.Replace(m.Groups["heading"].Value, string.Empty);
            return cleanH1;
        }
        private int? ExtractYear(string html)
        {
            var rgx = new Regex(@"\<ul\>\<li class\=\""head\""\>Year\<\/li\>\<\/ul\>\s*\<ul\>\<li\>(?<year>[0-9]{4})\<\/li\>\<\/ul\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (!m.Success) return null;

            return Convert.ToInt32(m.Groups["year"].Value);
        }
        private IEnumerable<Artist> ExtractArtists(string h1)
        {
            var rgx = new Regex(@"<a.*? href\=\""(?<url>.*?)\""\>(?<artist>.*?)\<\/a\>", RegexOptions.Singleline);
            var m_artists = rgx.Matches(h1);
            var arr_m_artists = new Match[m_artists.Count];
            m_artists.CopyTo(arr_m_artists, 0);

            if (!arr_m_artists.Any()) throw new Exception("Could not extract artists");

            return arr_m_artists.Select(m => new Artist
            {
                Url = "https://www.lololyrics.com" + m.Groups["url"].Value,
                Name = m.Groups["artist"].Value
            });
        }
        private string ExtractTitle(string h1)
        {
            var rgx = new Regex(@"<a.*? href\=\""(?<url>.*?)\""\>.*?\<\/a\>.*?-\s*(?<title>.*?)\<\/span\> lyrics", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(h1);
            if (!m.Success) throw new Exception("Could not extract title");

            return m.Groups["title"].Value;
        }
        private string ExtractSongLyrics(string html, bool trimTrash)
        {
            var rgx = new Regex(@"\<div class\=\""lyrics_txt\"".*?\>\s*(?<lyrics>.*?)\<\!--", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (!m.Success) throw new Exception("No lyrics found");

            var lyrics_dirty = m.Groups["lyrics"].Value;
            var rgx_comment = new Regex(@"\<\!--.+--\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var lyrics = rgx_comment.Replace(lyrics_dirty, string.Empty).Replace("<br />", string.Empty);

            if (!trimTrash) return lyrics;

            var rgx_repeats = new Regex(@"(?<line>.+) \[(?<count>[0-9]+)x\]");

            var m_repeat = rgx_repeats.Match(lyrics);
            while (m_repeat.Success)
            {
                lyrics = lyrics.Replace(m_repeat.Value, Repeat(m_repeat.Groups["line"].Value, int.Parse(m_repeat.Groups["count"].Value)));
                m_repeat = rgx_repeats.Match(lyrics);
            }

            return lyrics;
        }
        private string ExtractYoutubeId(string html)
        {
            var rgx = new Regex(@"loadYT\(this\, \'(?<youtube>.*?)\'\)\;", RegexOptions.Singleline);
            var m = rgx.Match(html);
            if (!m.Success) return null;

            return m.Groups["youtube"].Value;
        }

        private string Repeat(string input, int count)
        {
            return string.Join(Environment.NewLine, Enumerable.Repeat(input, count));
        }
    }
}
