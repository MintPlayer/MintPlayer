using MintPlayer.Fetcher.Abstractions.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MintPlayer.Fetcher.Musixmatch.Test")]
namespace MintPlayer.Fetcher.Musixmatch
{
	public interface IMusixmatchFetcher
	{
		Task<Subject> Fetch(string url, bool trimTrash);
	}

	internal class MusixmatchFetcher : Fetcher, IMusixmatchFetcher
	{
        private readonly HttpClient httpClient;
        public MusixmatchFetcher(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public override IEnumerable<Regex> UrlRegex
        {
            get
            {
                return new[] {
                    new Regex(@"https\:\/\/(www\.){0,1}musixmatch.com\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await SendRequest(httpClient, url);
            if (url.StartsWith("https://www.musixmatch.com/lyrics/"))
            {
                var ld_json = ReadLdJson(html);
                var song = await ParseSong(url, html, ld_json);
                return song;
            }
            else if (url.StartsWith("https://www.musixmatch.com/artist/"))
            {
                var artist = await ParseArtist(url, html);
                return artist;
            }
            else if (url.StartsWith("https://www.musixmatch.com/album/"))
            {
                var album = await ParseAlbum(url, html);
                return album;
            }
            else
            {
                throw new Exception();
            }
        }

        #region Private methods
        #region ParseSong
        private Task<Subject> ParseSong(string url, string html, string ldJson)
        {
            var song = JsonConvert.DeserializeObject<Data.Song>(ldJson);

            var artistRegex = new Regex(@"\<a href=\""(?<url>.*?)\"" class=\""mxm-track-title__artist mxm-track-title__artist-link\""\>(?<name>.*?)\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var artistMatches = artistRegex.Matches(html);
            var artistMatchesArr = new Match[artistMatches.Count];
            artistMatches.CopyTo(artistMatchesArr, 0);

            var result = new Song
            {
                Lyrics = ExtractSongLyrics(html, true),
                Title = song.Title,
                PrimaryArtist = new Artist
                {
                    Name = artistMatchesArr.First().Groups["name"].Value,
                    Url = "https://musixmatch.com" + artistMatchesArr.First().Groups["url"].Value
                },
                FeaturedArtists = artistMatchesArr.Skip(1).Select(m => new Artist
                {
                    Name = m.Groups["name"].Value,
                    Url = "https://musixmatch.com" + m.Groups["url"].Value
                }).ToList(),
                Url = url
            };
            return Task.FromResult<Subject>(result);
        }
        private string ReadLdJson(string html)
        {
            var ldJsonRegex = new Regex(@"\<script data-react-helmet=""true"" type=\""application\/ld\+json\"".*?\>(?<body>.*?)\<\/script\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var ldJsonMatch = ldJsonRegex.Match(html);
            if (!ldJsonMatch.Success) throw new Exception("No ld+json tag found");

            return ldJsonMatch.Groups["body"].Value;
        }
        private string ExtractSongLyrics(string html, bool trimTrash)
        {
            var spanRegex = new Regex(@"(?<=\<span class=\""lyrics__content__ok\""\>).*?(?=\<\/span\>)", RegexOptions.Singleline | RegexOptions.Multiline);
            var spanMatches = spanRegex.Matches(html);
            if (spanMatches.Count == 0) throw new Exception("span tag not found");

            var matches = new Match[spanMatches.Count];
            spanMatches.CopyTo(matches, 0);

            return string.Join("\r\n\r\n", matches.Select(m => m.Value));
        }
        #endregion
        #region ParseAlbum
        private Task<Subject> ParseAlbum(string url, string html)
        {
            var rgx_json = new Regex(@"var __mxmState = (?<json>.*?);\<\/script\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_json = rgx_json.Match(html);
            if (!m_json.Success) throw new Exception("ParseAlbum rgx match failed");

            Json.SongData data = null;
            try
            {
                data = JsonConvert.DeserializeObject<Json.SongData>(m_json.Groups["json"].Value);
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }

            var result = new Album
            {
                Id = data.Page.Album.Id,
                Name = data.Page.Album.Name,
                Artist = ExtractAlbumArtist(html),
                ReleaseDate = data.Page.Album.ReleaseDate,
                CoverArtUrl = FirstNotEmpty(data.Page.Album.CoverArt800, data.Page.Album.CoverArt500, data.Page.Album.CoverArt350, data.Page.Album.CoverArt100),
                Tracks = data.Page.Tracks.List.Select(t => new Song
                {
                    Id = t.Id,
                    Title = t.Name,
                    Url = t.ShareUrl.Split('?')[0]
                }).ToList(),
                Url = url
            };
            return Task.FromResult<Subject>(result);
        }
        private string FirstNotEmpty(params string[] items)
        {
            return items.FirstOrDefault(i => !string.IsNullOrEmpty(i));
        }
        private Artist ExtractAlbumArtist(string html)
        {
            var rgx_h2 = new Regex(@"\<h2 itemProp=\""byArtist\"".*?\>(?<artistinfo>.*?)\<\/h2\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var match_h2 = rgx_h2.Match(html);
            var rgx_info = new Regex(@"\<a href=\""(?<url>.*?)\""\>(?<name>.*?)\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var match_info = rgx_info.Match(match_h2.Value);
            return new Artist
            {
                Name = match_info.Groups["name"].Value,
                Url = "https://musixmatch.com" + match_info.Groups["url"].Value
            };
        }
        #endregion
        #region ParseArtist
        private async Task<Subject> ParseArtist(string url, string html)
        {
            // Extract albums
            // /album/The-Weeknd-3/Blinding-Lights
            var albums = await ExtractArtistAlbums(url);

            // Join songs for all albums
            var fetched_albums = await Task.WhenAll(albums.Select(album => Fetch(album.Url, true)));
            var songs = fetched_albums
                .Cast<Album>()
                .SelectMany(a => a.Tracks);

            var artist = new Artist
            {
                Name = ExtractArtistName(html),
                Url = url,
                ImageUrl = ExtractArtistImage(html),
                Albums = albums.ToList(),
                Songs = songs.ToList()
            };
            return artist;
        }
        private string ExtractArtistName(string html)
        {
            var rgx_h1 = new Regex(@"\<h1 title=\"".*?\"" .*?\>(?<name>.*?)\<\/h1\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var match_h1 = rgx_h1.Match(html);
            if (match_h1.Success) return match_h1.Groups["name"].Value;
            else return null;
        }
        private string ExtractArtistImage(string html)
        {
            // <img itemprop="image" class="profile-avatar large" src="https://static.musixmatch.com/images-storage/mxmimages/3/6/2/7/9/97263_14.jpg" alt="The Weeknd - lyrics">
            var rgx_image = new Regex(@"\<img itemprop=\""image\"" class=\""profile\-avatar large\"" src=\""(?<image>.*?)\"" alt=\"".*?\""\>", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var match_image = rgx_image.Match(html);
            if (match_image.Success) return match_image.Groups["image"].Value;
            else return null;
        }
        private async Task<IEnumerable<Album>> ExtractArtistAlbums(string url)
        {
            var link = url;
            if (!link.EndsWith("/")) link += "/";
            link += "albums";

            var html = await SendRequest(httpClient, link);

            var rgx_albums_ul = new Regex(@"\<ul class=\""albums grid\""\>(?<list>.*?)\<\/ul\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_albums_ul = rgx_albums_ul.Match(html);
            if (!m_albums_ul.Success) throw new Exception("No ul tag found");

            var rgx_album_li = new Regex(@"\<li\>.*?\<h2 class=\""media\-card\-title\""\>\<a href=\""(?<url>.*?)\""\>(?<title>.*?)\<\/a\>\<\/h2\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_albums = rgx_album_li.Matches(m_albums_ul.Groups["list"].Value);
            var arr_albums = new Match[m_albums.Count];
            m_albums.CopyTo(arr_albums, 0);

            return arr_albums.Select(a => new Album
            {
                Name = a.Groups["title"].Value,
                Url = "https://www.musixmatch.com" + System.Web.HttpUtility.HtmlDecode(a.Groups["url"].Value)
            });
        }
        #endregion
        #endregion
    }
}
