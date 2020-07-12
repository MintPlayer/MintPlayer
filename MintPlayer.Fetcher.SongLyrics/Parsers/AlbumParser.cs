using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.SongLyrics.Parsers
{
    internal interface IAlbumParser
    {
        Task<Subject> ParseAlbum(string url, string h1, string html);
    }

    internal class AlbumParser : IAlbumParser
    {
        public Task<Subject> ParseAlbum(string url, string h1, string html)
        {
            return Task.FromResult<Subject>(new Album
            {
                Url = url,
                Name = ExtractAlbumName(html),
                Tracks = ExtractAlbumSongs(html),
                Artist = ExtractAlbumArtist(html),
                CoverArtUrl = ExtractAlbumCover(html)
            });
        }
        private string ExtractAlbumName(string html)
        {
            var rgx = new Regex(@"\<li class\=\""current\""\>(?<name>.*?) Album\<\/li\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (!m.Success) throw new Exception("Could not extract album name");

            return m.Groups["name"].Value;
        }
        private Artist ExtractAlbumArtist(string html)
        {
            var rgx = new Regex(@"\<p\>Artist\: \<a href\=\""(?<artist_url>.*?)\"".*?\>(?<artist_name>.*?)\<\/a\>\<\/p\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (!m.Success) throw new Exception("Could not extract album artist");

            return new Artist
            {
                Url = m.Groups["artist_url"].Value,
                Name = m.Groups["artist_name"].Value
            };
        }
        private List<Song> ExtractAlbumSongs(string html)
        {
            var rgx = new Regex(@"\<div class\=\""rightcol rightcol\-thin\""\>\s*\<table class\=\""tracklist\""\>\s*\<tbody\>\s*(?<songs>.*?)\<\/tbody\>\s*\<\/table\>", RegexOptions.Singleline | RegexOptions.Multiline);
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
        private string ExtractAlbumCover(string html)
        {
            var rgx = new Regex(@"\<div class\=\""leftcol leftcol\-thin\""\>\s*\<img src\=\""(?<image>.*?)\"".*?\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (!m.Success) throw new Exception("No album cover found");

            return m.Groups["image"].Value;
        }
    }
}
