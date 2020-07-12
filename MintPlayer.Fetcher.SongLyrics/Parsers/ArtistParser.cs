using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.SongLyrics.Parsers
{
    internal interface IArtistParser
    {
        Task<Subject> ParseArtist(string url, string h1, string html);
    }

    internal class ArtistParser : IArtistParser
    {
        public Task<Subject> ParseArtist(string url, string h1, string html)
        {
            return Task.FromResult<Subject>(new Artist
            {
                Url = url,
                Name = h1.Substring(0, h1.Length - 12),
                Songs = ExtractArtistSongs(html),
                Albums = ExtractArtistAlbums(html)
            });
        }
        private List<Song> ExtractArtistSongs(string html)
        {
            var rgx = new Regex(@"\<h2 itemprop\=\""name\""\>.*?\<\/h2\>\s*\<table class\=\""tracklist\""\>\s*\<tbody\>\s*(?<songs>.*?)\<\/tbody\>\s*\<\/table\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_table = rgx.Match(html);
            if (!m_table.Success) throw new Exception("No songs found");

            var rgx_tr = new Regex(@"\<tr.*?\>.*?\<a.*?href=\""(?<url>.*?)\"".*?\>(?<title>.*?)\<\/a\>.*?\<\/td\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_tr = rgx_tr.Matches(m_table.Groups["songs"].Value);
            var arr_tr = new Match[m_tr.Count];
            m_tr.CopyTo(arr_tr, 0);

            return arr_tr.Select(m => new Song
            {
                Url = m.Groups["url"].Value,
                Title = m.Groups["title"].Value
            }).ToList();
        }
        private List<Album> ExtractArtistAlbums(string html)
        {
            var rgx = new Regex(@"\<div class\=\""listbox\-album\""\>\s*\<a href\=\""(?<url>.*?)\"".*?\>\s*\<img src\=\""(?<image>.*?)\"".*?\>\s*\<\/a\>\s*\<h3\>\<a href\=\""\k<url>\"".*?\>(?<title>.*?)\<\/a\>\<\/h3\>\s*\<\/div\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var matches = rgx.Matches(html);
            var arrM = new Match[matches.Count];
            matches.CopyTo(arrM, 0);

            return arrM.Select(m => new Album
            {
                Url = m.Groups["url"].Value,
                Name = m.Groups["title"].Value,
                CoverArtUrl = m.Groups["image"].Value
            }).ToList();
        }
    }
}
