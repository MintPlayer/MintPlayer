using MintPlayer.Fetcher.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Musixmatch.Parsers
{
    internal interface IArtistParser
    {
        Task<Subject> ParseArtist(string url, string html);
    }
    internal class ArtistParser : IArtistParser
    {
        private readonly IRequestSender requestSender;
        private readonly IAlbumParser albumParser;
        public ArtistParser(IRequestSender requestSender, IAlbumParser albumParser)
        {
            this.requestSender = requestSender;
            this.albumParser = albumParser;
        }

        public async Task<Subject> ParseArtist(string url, string html)
        {
            // Extract albums
            // /album/The-Weeknd-3/Blinding-Lights
            var albums = await ExtractArtistAlbums(url);

            // Join songs for all albums
            var fetched_albums = await Task.WhenAll(albums.Select(album =>
                requestSender
                    .SendRequest(album.Url)
                    .ContinueWith(htmlTask => albumParser.ParseAlbum(album.Url, htmlTask.Result))
            ));

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

            var html = await requestSender.SendRequest(link);

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
    }
}
