using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.SongtekstenNet.Parsers
{
    internal interface IAlbumParser
    {
        Task<Subject> ParseAlbum(string url, string html);
    }

    internal class AlbumParser : IAlbumParser
    {
        public Task<Subject> ParseAlbum(string url, string html)
        {
            var url_splitted = url.Split('/');
            var artists = ExtractAlbumArtists(html);
            var album = new Album
            {
                Url = url,
                Id = Convert.ToInt32(url_splitted[url_splitted.Length - 3]),
                Name = ExtractAlbumTitle(html),
                Artist = artists.Count == 1 ? artists.First().ToDto() : null,
                Tracks = ExtractAlbumSongs(html).Select(s => s.ToDto()).ToList(),
                CoverArtUrl = ExtractCoverArtUrl(html),
                ReleaseDate = ExtractReleaseDate(html)
            };
            return Task.FromResult<Subject>(album);
        }

        private string ExtractAlbumTitle(string html)
        {
            var h1Regex = new Regex(@"\<h1.*?\>(?<title>.*?)\<\/h1\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var h1Match = h1Regex.Match(html);
            if (!h1Match.Success) throw new Exception("No H1 tag found");

            var title = h1Match.Groups["title"].Value;
            return title.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries).First();
        }
        private List<Data.Song> ExtractAlbumSongs(string html)
        {
            var songsRegex = new Regex(@"\<h1\>(?<artist_album>.*?)\<\/h1\>\s*\<ul class\=\""list\-unstyled\""\>\s*(?<tracks>.*?)\s*\<\/ul\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var matchSongs = songsRegex.Match(html);
            if (!matchSongs.Success) throw new Exception("No ul found");

            var liRegex = new Regex(@"\<li.*?\>.*?\<a href\=\""(?<url>.*?)\""\>(?<title>.*?)\<\/a\>\<\/li\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var songMatches = liRegex.Matches(matchSongs.Groups["tracks"].Value);
            var arr_matches = new Match[songMatches.Count];
            songMatches.CopyTo(arr_matches, 0);

            return arr_matches.Select(m =>
            {
                var url_split = m.Groups["url"].Value.Split('/');
                return new Data.Song
                {
                    Id = Convert.ToInt32(url_split[url_split.Length - 3]),
                    Url = m.Groups["url"].Value,
                    Title = m.Groups["title"].Value
                };
            }).ToList();
        }
        private List<Data.Artist> ExtractAlbumArtists(string html)
        {
            var artistsRegex = new Regex(@"\<h3\>Artiesten\<\/h3\>\s*\<ul class\=\""list\-unstyled\""\>\s*(?<artists>.*?)\s*\<\/ul\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var matchArtists = artistsRegex.Match(html);
            if (!matchArtists.Success) throw new Exception("No ul found");

            var liRegex = new Regex(@"\<li.*?\>.*?\<a href\=\""(?<url>.*?)\""\>(?<name>.*?)\<\/a\>\<\/li\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var artistMatches = liRegex.Matches(matchArtists.Groups["artists"].Value);
            var arr_matches = new Match[artistMatches.Count];
            artistMatches.CopyTo(arr_matches, 0);

            return arr_matches.Select(m =>
            {
                var url_split = m.Groups["url"].Value.Split('/');
                return new Data.Artist
                {
                    Id = Convert.ToInt32(url_split[url_split.Length - 2]),
                    Url = m.Groups["url"].Value,
                    Name = m.Groups["name"].Value
                };
            }).ToList();
        }
        private string ExtractCoverArtUrl(string html)
        {
            var regexCover = new Regex(@"\<img class\=\""img\-thumbnail img\-fullwidth\"" src\=\""(?<cover>.*?)\"" alt\="".*?\"" border\=\""0\""\s*\/>", RegexOptions.Singleline | RegexOptions.Multiline);
            var matchCover = regexCover.Match(html);
            if (!matchCover.Success) throw new Exception("No album cover found");

            return matchCover.Groups["cover"].Value;
        }
        private DateTime ExtractReleaseDate(string html)
        {
            var regexStats = new Regex(@"\<h3\>Statistieken\<\/h3\>\s*\<ul class\=\""list\-unstyled\""\>\s*(?<stats>.*?)\s*\<\/ul\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var matchStats = regexStats.Match(html);
            if (!matchStats.Success) throw new Exception("No stats found");

            var liRegex = new Regex(@"\<li\>\<span class\=\""glyphicon glyphicon\-star\""\>\<\/span\>\s*(?<property>[\sA-Za-z]+?)\:\s*(?<value>.+?)\s*\<\/li\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var liMatches = liRegex.Matches(matchStats.Value);
            var arr_matches = new Match[liMatches.Count];
            liMatches.CopyTo(arr_matches, 0);

            var dict = arr_matches.ToDictionary(m => m.Groups["property"].Value, m => m.Groups["value"].Value);
            return DateTime.ParseExact(dict["Release"], "dd-MM-yyyy", null);
        }
    }
}
