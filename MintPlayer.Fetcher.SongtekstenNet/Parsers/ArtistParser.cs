using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.SongtekstenNet.Parsers
{
    internal interface IArtistParser
    {
        Task<Subject> ParseArtist(string url, string html);
    }

    internal class ArtistParser : IArtistParser
    {
        public Task<Subject> ParseArtist(string url, string html)
        {
            var splitted = url.Split('/');
            var artist = new Artist
            {
                Url = url,
                Id = Convert.ToInt32(splitted[splitted.Length - 2]),
                Name = ExtractArtistName(html),
                Songs = ExtractArtistSongs(html).Select(s => s.ToDto()).ToList(),
                Albums = ExtractArtistAlbums(html).Select(a => a.ToDto()).ToList()
            };
            return Task.FromResult<Subject>(artist);
        }

        private string ExtractArtistName(string html)
        {
            var breadcrumbRegex = new Regex(@"(?<=\<ol class=\""breadcrumb\""\>).*?(?=\<\/ol\>)", RegexOptions.Singleline | RegexOptions.Multiline);
            var breadcrumbMatch = breadcrumbRegex.Match(html);
            if (!breadcrumbMatch.Success) throw new Exception("No breadcrumb found");

            var bc = breadcrumbMatch.Value;
            var liRegex = new Regex(@"(?<=\<li\>).*?(?=\<\/li\>)", RegexOptions.Singleline | RegexOptions.Multiline);
            var liMatches = liRegex.Matches(bc);
            if (liMatches.Count == 0) throw new Exception("No title found");

            var a = liMatches[liMatches.Count - 2].Value;
            var aRegex = new Regex(@"(?<=\<a.*?\>).*?(?=\<\/a\>)", RegexOptions.Singleline | RegexOptions.Multiline);
            var aMatch = aRegex.Match(a);
            if (!aMatch.Success) throw new Exception("No a tag found");

            return aMatch.Value;
        }
        private List<Data.Song> ExtractArtistSongs(string html)
        {
            var h1Regex = new Regex(@"\<\/h1\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var main = h1Regex.Split(html)[1];

            var ulRegex = new Regex(@"\<ul class\=\""list-unstyled\""\>(?<lis>.*?)\<\/ul\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var songsMatch = ulRegex.Match(main).Groups["lis"];

            var songRegex = new Regex(@"\<a href=\""(?<url>.*?)\"">(?<title>.*?)\<\/a\>");
            var songMatches = songRegex.Matches(songsMatch.Value);
            var arrSongsMatches = new Match[songMatches.Count];
            songMatches.CopyTo(arrSongsMatches, 0);

            return arrSongsMatches.Select(m => {
                var parts = m.Groups["url"].Value.Split('/');
                return new Data.Song
                {
                    Id = Convert.ToInt32(parts[parts.Length - 3]),
                    Title = m.Groups["title"].Value,
                    Url = m.Groups["url"].Value
                };
            }).ToList();
        }
        private List<Data.Album> ExtractArtistAlbums(string html)
        {
            var albumsRegex = new Regex(@"\<h3\>Albums\<\/h3\>\s*\<ul class\=\""list-unstyled\""\>(?<lis>.*?)\<\/ul\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var albumsSection = albumsRegex.Match(html).Groups["lis"].Value;

            var albumRegex = new Regex(@"\<a href=\""(?<url>.*?)\"">(?<name>.*?)\s*\((?<released>[^)]+)\)<\/a\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var albumMatches = albumRegex.Matches(albumsSection);
            var arrAlbumMatches = new Match[albumMatches.Count];
            albumMatches.CopyTo(arrAlbumMatches, 0);

            return arrAlbumMatches.Select(m =>
            {
                var parts = m.Groups["url"].Value.Split('/');
                return new Data.Album
                {
                    Id = Convert.ToInt32(parts[parts.Length - 3]),
                    Url = m.Groups["url"].Value,
                    Name = m.Groups["name"].Value,
                    Released = DateTime.ParseExact(m.Groups["released"].Value, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture)
                };
            }).ToList();
        }
    }
}
