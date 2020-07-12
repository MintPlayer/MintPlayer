using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.LyricsCom.Parsers
{
    internal interface IArtistParser
    {
        Task<Subject> ParseArtist(string url, string html);
    }
    internal class ArtistParser : IArtistParser
    {
        public Task<Subject> ParseArtist(string url, string html)
        {
            var albums = ExtractArtistAlbums(url, html);

            var result = new Artist
            {
                Id = ExtractArtistId(url),
                Name = ExtractArtistName(html),
                Url = url,
                ImageUrl = ExtractArtistImage(html),
                Albums = albums.ToList(),
                Songs = albums.SelectMany(a => a.Tracks).ToList()
            };

            return Task.FromResult<Subject>(result);
        }

        private int ExtractArtistId(string url)
        {
            var parts = url.Split('/');

            int result;
            var success = int.TryParse(parts.Last(), out result);
            if (!success) Debugger.Break();
            return result;
        }

        private string ExtractArtistName(string html)
        {
            var h1Regex = new Regex(@"\<h1.*?\>\<a href=\""(?<url>.*?)\>(?<name>.*?)\<\/a\>\<\/h1\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var h1Match = h1Regex.Match(html);
            if (!h1Match.Success) throw new Exception("No H1 tag found");

            return h1Match.Groups["name"].Value;
        }

        private string ExtractArtistImage(string html)
        {
            var rgx_div_avatar = new Regex(@"\<div id=\""featured\-artist\-avatar\"" .*?\>\<img src=\""(?<image>.*?)\"" class=\""artist\-thumb\""\>\<\/div\>", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var match_image = rgx_div_avatar.Match(html);
            if (match_image.Success) return match_image.Groups["image"].Value;
            else return null;
        }

        private IEnumerable<Album> ExtractArtistAlbums(string url, string html)
        {
            var rgx_container = new Regex(@"\<div class=\""tdata\-ext\""\>\s*(?<albums>.*?)\s*\<\/div\>\s*\<section.*?\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var match_albums = rgx_container.Match(html);
            if (!match_albums.Success) throw new Exception("No albums section found");

            var rgx_album = new Regex(@"\<div class=\""clearfix\""\>\<h3 class=\""artist\-album\-label\""\>\<a href=\""(?<album_url>.*?)\""\>(?<album_title>.*?)\<\/a\>\s*\<span class=\""year\""\>\[(?<album_year>[0-9]{4})\]\<\/span\>\<\/h3\>\<div class=\""artist\-album\-thumb\""\>\<img src=\""(?<album_image>.*?)\""\>\<\/div\>\<table class=\""tdata\""\>.*?\<tbody\>(?<rows>.*?)\<\/tbody\>\<\/table\>\<\/div\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var album_matches = rgx_album.Matches(match_albums.Groups["albums"].Value);
            var arr_albums = new Match[album_matches.Count];
            album_matches.CopyTo(arr_albums, 0);

            return arr_albums.Select(a => new Album
            {
                Id = Convert.ToInt32(a.Groups["album_url"].Value.Split('/').Reverse().ElementAt(1)),
                Url = "https://www.lyrics.com" + a.Groups["album_url"].Value,
                //Url = "https://www.lyrics.com" + HttpUtility.UrlDecode(a.Groups["album_url"].Value),
                Name = a.Groups["album_title"].Value,
                ReleaseDate = new DateTime(Convert.ToInt32(a.Groups["album_year"].Value), 1, 1),
                CoverArtUrl = a.Groups["album_image"].Value,

                Tracks = ExtractArtistAlbumSongs(a.Groups["rows"].Value).ToList()
            });
        }

        private IEnumerable<Song> ExtractArtistAlbumSongs(string html)
        {
            var rgx_song = new Regex(@"\<tr\>\<td.*?\>\<strong\>\<a href=\""(?<url>.*?)\""\>(?<title>.*?)\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_song = rgx_song.Matches(html);
            var matches = new Match[m_song.Count];
            m_song.CopyTo(matches, 0);

            return matches.Select(m => new Song
            {
                Title = m.Groups["title"].Value,
                Url = "https://www.lyrics.com" + m.Groups["url"].Value
                //Url = "https://www.lyrics.com" + HttpUtility.UrlDecode(m.Groups["url"].Value)
            });
        }
    }
}
