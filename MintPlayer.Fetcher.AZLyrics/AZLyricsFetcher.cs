using MintPlayer.Fetcher.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.AZLyrics
{
    public class AZLyricsFetcher : Fetcher
    {
        private readonly HttpClient httpClient;
        public AZLyricsFetcher(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public override IEnumerable<Regex> UrlRegex
        {
            get
            {
                return new[]
                {
                    new Regex(@"https:\/\/www\.azlyrics\.com\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await SendRequest(httpClient, url);
            if (url.StartsWith("https://www.azlyrics.com/lyrics/"))
            {
                var song = await ParseSong(url, html, trimTrash);
                return song;
            }
            else if(Regex.IsMatch(url, @"https\:\/\/www\.azlyrics\.com\/[0-9a-z]+\/[0-9a-z]+\.html"))
            {
                var artist = await ParseArtist(url, html);
                return artist;
            }
            else
            {
                throw new Exception();
            }
        }


        #region Private methods
        #region ParseSong
        private async Task<Subject> ParseSong(string url, string html, bool trimTrash)
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

            if(!trimTrash)
                return Task.FromResult(m.Groups["lyrics"].Value);

            var rgxI = new Regex(@"\<i\>.*?\<\/i\>\<br\>\n", RegexOptions.Singleline | RegexOptions.Multiline);
            var trimmed = rgxI.Replace(m.Groups["lyrics"].Value, string.Empty);

            var result = trimmed.Replace("\r", "").Replace("\n", "").Replace("<br>", Environment.NewLine);

            return Task.FromResult(result);
        }
        #endregion
        #region ParseArtist
        private async Task<Subject> ParseArtist(string url, string html)
        {
            var albums = await ExtractArtistAlbums(html);
            return new Artist
            {
                Url = url,
                Name = await ExtractArtistName(html),
                Albums = albums.ToList(),
                Songs = albums.SelectMany(a => a.Tracks).ToList()
            };
        }
        private Task<string> ExtractArtistName(string html)
        {
            var rgx = new Regex(@"\<h1\>\<strong\>(?<artist>.+) Lyrics\<\/strong\>\<\/h1\>");
            var m = rgx.Match(html);
            if (!m.Success) throw new Exception("Could not extract artist name");

            var name = m.Groups["artist"].Value;
            return Task.FromResult(name);
        }
        private Task<Album[]> ExtractArtistAlbums(string html)
        {
            var rgx = new Regex(@"\<div id\=\""[0-9]+\"" class\=\""album\""\>album\: \<b\>\""(?<album_name>.*?)\""\<\/b\> \((?<year>[0-9]{4})\)\<\/div\>\s(?<tracks>.*?)\s\s", RegexOptions.Singleline | RegexOptions.Multiline);
            var matches = rgx.Matches(html);
            var arrMatches = new Match[matches.Count];
            matches.CopyTo(arrMatches, 0);

            var tasks = arrMatches.Select((m) => {
                int year;
                var hasYear = int.TryParse(m.Groups["year"].Value, out year);
                
                return Task.Run(async () =>
                {
                    return new Album
                    {
                        Url = null,
                        Name = m.Groups["album_name"].Value,
                        ReleaseDate = hasYear ? (DateTime?)new DateTime(year, 1, 1) : null,
                        Tracks = await ExtractArtistAlbumTracks(m.Groups["tracks"].Value)
                    };
                });
            });

            return Task.WhenAll(tasks);
        }
        private Task<List<Song>> ExtractArtistAlbumTracks(string html)
        {
            var rgx = new Regex(@"\<div class\=\""listalbum\-item\""\>\<a href\=\""\.\.(?<url>.*?)\"" target\=\""_blank\""\>(?<title>.*?)\<\/a\>\<\/div\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var matches = rgx.Matches(html);
            var arrMatches = new Match[matches.Count];
            matches.CopyTo(arrMatches, 0);

            return Task.FromResult(arrMatches.Select(m => new Song
            {
                Url = "https://www.azlyrics.com" + m.Groups["url"].Value,
                Title = m.Groups["title"].Value
            }).ToList());
        }
        #endregion
        #endregion
    }
}
