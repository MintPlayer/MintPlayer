using MintPlayer.Fetcher.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.SongLyrics
{
    public class SongLyricsFetcher : Fetcher
    {
        private readonly HttpClient httpClient;
        public SongLyricsFetcher(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public override IEnumerable<Regex> UrlRegex
        {
            get
            {
                return new[] {
                    new Regex(@"http:\/\/www\.songlyrics\.com\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await SendRequest(httpClient, url);
            var h1 = ReadH1(html);
            if (IsArtist(h1, html))
            {
                var artist = await ParseArtist(url, h1, html);
                return artist;
            }
            else if (IsSong(h1, html))
            {
                var song = await ParseSong(url, h1, html);
                return song;
            }
            else if (IsAlbum(h1, html))
            {
                var album = await ParseAlbum(url, h1, html);
                return album;
            }
            else
            {
                throw new Exception("Could not determine subject type");
            }
        }

        #region Helper methods
        private string ReadH1(string html)
        {
            var rgx = new Regex(@"\<h1\>(?<title>.*)\<\/h1\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (!m.Success) throw new Exception("No h1 tag found");

            return m.Groups["title"].Value;
        }
        #endregion
        #region Distinct methods
        private bool IsAlbum(string h1, string html)
        {
            return h1.EndsWith(" Album");
        }
        private bool IsSong(string h1, string html)
        {
            var rgxArtist = new Regex(@"\<p\>Artist\: \<a href\=\"".*?\"" title\=\"".*?\""\>.*?\<\/a\>\<\/p\>", RegexOptions.Singleline);
            return h1.EndsWith(" Lyrics") & rgxArtist.IsMatch(html);
        }
        private bool IsArtist(string h1, string html)
        {
            var rgxArtist = new Regex(@"\<p\>Artist\: \<a href\=\"".*?\"" title\=\"".*?\""\>.*?\<\/a\>\<\/p\>", RegexOptions.Singleline);
            return h1.EndsWith(" Song Lyrics") & !rgxArtist.IsMatch(html);
        }
        #endregion
        #region Private methods
        #region ParseSong
        private Task<Subject> ParseSong(string url, string h1, string html)
        {
            return Task.FromResult<Subject>(new Song
            {
                Url = url,
                //PrimaryArtist = ExtractSongArtist(html),
                Title = ExtractSongTitle(h1, html),
                Lyrics = ExtractSongLyrics(html)
            });
        }
        private string ExtractSongTitle(string h1, string html)
        {
            var artist = ExtractSongArtist(html);
            var title = h1.Substring(artist.Name.Length + 3);
            return title.Substring(0, title.Length - 7);
        }
        private Artist ExtractSongArtist(string html)
        {
            var rgxArtist = new Regex(@"\<p\>Artist\: \<a href\=\""(?<url>.*?)\"" title\=\"".*?\""\>(?<name>.*?)\<\/a\>\<\/p\>", RegexOptions.Singleline);
            var m = rgxArtist.Match(html);
            if (!m.Success) throw new Exception("No artist found");

            return new Artist
            {
                Url = m.Groups["url"].Value,
                Name = m.Groups["name"].Value
            };
        }
        private string ExtractSongLyrics(string html)
        {
            var rgx = new Regex(@"\<p id\=\""songLyricsDiv\"".*?\>(?<lyrics>.*?)\<\/p\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (!m.Success) throw new Exception("Could not extract the song lyrics");

            var trimmed = m.Groups["lyrics"].Value.Replace("\r", string.Empty).Replace("\n", string.Empty);

            var rgxBr = new Regex(@"\<br.*?\>", RegexOptions.Singleline);
            var result = rgxBr.Replace(trimmed, Environment.NewLine);
            return result;
        }
        #endregion
        #region ParseArtist
        private Task<Subject> ParseArtist(string url, string h1, string html)
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
        #endregion
        #region ParseAlbum
        private Task<Subject> ParseAlbum(string url, string h1, string html)
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
        #endregion
        #endregion
    }
}
