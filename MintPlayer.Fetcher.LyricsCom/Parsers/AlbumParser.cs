using System;
using System.Web;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.LyricsCom.Parsers
{
    internal interface IAlbumParser
    {
        Task<Subject> ParseAlbum(string url, string html);
    }
    internal class AlbumParser : IAlbumParser
    {
        public Task<Subject> ParseAlbum(string url, string html)
        {
            var year = ExtractReleasedYear(html);

            var result = new Album
            {
                Id = Convert.ToInt32(url.Split('/').Reverse().ElementAt(1)),
                Url = url,
                Name = ExtractAlbumTitle(html),
                Artist = ExtractAlbumArtist(html),
                CoverArtUrl = ExtractAlbumCoverArt(html),
                ReleaseDate = year == 0 ? (DateTime?)null : new DateTime(year, 1, 1),
                Tracks = ExtractAlbumTracks(html).ToList()
            };

            return Task.FromResult<Subject>(result);
        }

        private string ExtractAlbumTitle(string html)
        {
            var rgx_title = new Regex(@"\<h1\>\<strong\>(?<title>.*) Album\<\/strong\>\<\/h1\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_title = rgx_title.Match(html);

            if (m_title.Success) return m_title.Groups["title"].Value;
            else return null;
        }

        private IEnumerable<Song> ExtractAlbumTracks(string html)
        {
            var rgx_table = new Regex(@"\<tbody\>(?<rows>.*)\<\/tbody\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_table = rgx_table.Match(html);

            if (!m_table.Success) throw new Exception("Track table not found");

            var rgx_track = new Regex(@"\<tr\>\<td .*?\>(?<disc_number>[0-9]+)\<\/td\>\<td .*?\>(?<track_number>[0-9]+)\<\/td\>\<td .*?\>\<div\>\<strong\>\<a href=\""(?<url>.*?)\""\>(?<title>.*?)\<\/a\>\<\/strong\>\<\/div\>\<\/td\>\<td .*?\>(?<duration>[0-9\:]+)\<\/td\>\<td .*?\>.*?\<\/td\>\<\/tr\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_tracks = rgx_track.Matches(m_table.Groups["rows"].Value);
            var track_matches = new Match[m_tracks.Count];
            m_tracks.CopyTo(track_matches, 0);

            return track_matches.Select(m => new Song
            {
                Id = Convert.ToInt32(m.Groups["url"].Value.Split('/').Reverse().ElementAt(1)),
                Url = "https://www.lyrics.com" + m.Groups["url"].Value,
                Title = m.Groups["title"].Value
            });
        }

        private Artist ExtractAlbumArtist(string html)
        {
            var rgx_artist = new Regex(@"\<h1\>.*\<\/h1\>\s*\<h2\>(?<artist>.*?)\<\/h2\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_artist = rgx_artist.Match(html);

            if (!m_artist.Success) throw new Exception("No h2 tag found");

            var content = m_artist.Groups["artist"].Value;
            if (content == "Various Artists") return null;

            var rgx_a = new Regex(@"\<a href=\""(?<url>.*?)\""\>(?<name>.*?)\<\/a\>", RegexOptions.Singleline);
            var m_a = rgx_a.Match(content);

            if (!m_a.Success) throw new Exception("Pattern match for album artist failed");

            return new Artist
            {
                Name = m_a.Groups["name"].Value,
                Url = "https://www.lyrics.com/" + HttpUtility.UrlDecode(m_a.Groups["url"].Value)
            };
        }

        private string ExtractAlbumCoverArt(string html)
        {
            var rgx_cover = new Regex(@"\<div class=\""album\-meta\-thumb\"" .*?\>\<img .*? src=\""(?<image>.*?)"" .*?\>\<\/div\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_cover = rgx_cover.Match(html);

            if (m_cover.Success) return m_cover.Groups["image"].Value;
            else return null;
        }

        private int ExtractReleasedYear(string html)
        {
            var rgx_year = new Regex(@"\<a .*?\>(?<year>[0-9]{4})\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_year = rgx_year.Match(html);

            if (m_year.Success) return Convert.ToInt32(m_year.Groups["year"].Value);
            else return 0;
        }
    }
}
