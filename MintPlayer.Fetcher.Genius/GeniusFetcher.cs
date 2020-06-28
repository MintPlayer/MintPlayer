using MintPlayer.Fetcher.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius
{
    public class GeniusFetcher : Fetcher
    {
        private readonly HttpClient httpClient;
        public GeniusFetcher(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public override IEnumerable<Regex> UrlRegex
        {
            get
            {
                return new[] {
                    new Regex(@"https:\/\/genius\.com\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await SendRequest(httpClient, url);
            var page_data = await ReadPageData(html);

            var structure = new { currentPage = string.Empty };
            var subject = JsonConvert.DeserializeAnonymousType(page_data, structure);

            switch (subject.currentPage)
            {
                case "profile":
                    return await ParseArtist(page_data);
                case "songPage":
                    return await ParseSong(page_data, trimTrash);
                case "album":
                    return await ParseAlbum(page_data);
                default:
                    throw new Exception("Type not recognized");
            }
        }

        #region Private methods
        private Task<string> ReadPageData(string html)
        {
            //var pageDataRegex = new Regex(@"(?<=\<meta content\=\"")(.*?)(?=\""\sitemprop\=\""page_data\""\>\<\/meta\>)");
            var pageDataRegex = new Regex(@"window\.__PRELOADED_STATE__\s\=\sJSON\.parse\(\'(?<data>.*?)\'\)\;");

            var pageData = pageDataRegex.Match(html).Groups["data"].Value;
            var fixedPageData = pageData
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("&quot;", "\"")
                .Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&#39;", "'");

            return Task.FromResult(fixedPageData);
        }
        private string ExtractLyrics(string pageDataBodyHtml, bool trimTrash)
        {
            var pRegex = new Regex(@"(?<=\<p\>).*?(?=\<\/p\>)", RegexOptions.Singleline | RegexOptions.Multiline);
            var pMatch = pRegex.Match(pageDataBodyHtml);
            if (!pMatch.Success) throw new Exception("No P tag found");

            var stripARegex = new Regex(@"\<a.*?\>|\<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var stripped = stripARegex.Replace(pMatch.Value, "");
            var whitespaces_stripped = stripped.Replace("\r", "").Replace("\n", "").Replace("<br>", Environment.NewLine);

            if (trimTrash)
            {
                var stripBracketsRegex = new Regex(@"\[.*?\]\r\n", RegexOptions.Singleline | RegexOptions.Multiline);
                var brackets_stripped = stripBracketsRegex.Replace(whitespaces_stripped, "");
                return brackets_stripped;
            }
            else
            {
                return whitespaces_stripped;
            }
        }
        private async Task<Subject> ParseArtist(string pageData)
        {
            var data = JsonConvert.DeserializeObject<Data.ArtistData>(pageData);

            var songs = new List<Data.Song>();
            var albums = new List<Data.Album>();
            var page = (int?)1;
            var songs_structure = new
            {
                meta = new
                {
                    status = 0
                },
                response = new
                {
                    next_page = (int?)0,
                    songs = new List<Data.Song>()
                }
            };
            var albums_structure = new
            {
                meta = new
                {
                    status = 0
                },
                response = new
                {
                    next_page = (int?)0,
                    albums = new List<Data.Album>()
                }
            };

            while (true)
            {
                var response = await httpClient.GetAsync($"https://genius.com/api/artists/{data.Artist.Id}/songs?per_page=50&page={page}&sort=popularity");
                var json_songs = await response.Content.ReadAsStringAsync();
                var data_songs = JsonConvert.DeserializeAnonymousType(json_songs, songs_structure);
                songs.AddRange(data_songs.response.songs);

                if ((page = data_songs.response.next_page) == null)
                    break;
            }

            page = 1;
            while (true)
            {
                var response = await httpClient.GetAsync($"https://genius.com/api/artists/{data.Artist.Id}/albums?per_page=50&page={page}");
                var json_albums = await response.Content.ReadAsStringAsync();
                var data_albums = JsonConvert.DeserializeAnonymousType(json_albums, albums_structure);
                albums.AddRange(data_albums.response.albums);

                if ((page = data_albums.response.next_page) == null)
                    break;
            }

            data.Songs = songs;
            data.Albums = albums;

            return data.ToDto();
        }
        private Task<Subject> ParseSong(string pageData, bool trimTrash)
        {
            var data = JsonConvert.DeserializeObject<Data.SongData>(pageData);
            data.Song.Lyrics = ExtractLyrics(data.LyricsData.Body.Html, trimTrash);
            return Task.FromResult<Subject>(data.Song.ToDto());
        }
        private Task<Subject> ParseAlbum(string pageData)
        {
            var data = JsonConvert.DeserializeObject<Data.AlbumData>(pageData);
            return Task.FromResult<Subject>(data.ToDto());
        }
        #endregion
    }
}
