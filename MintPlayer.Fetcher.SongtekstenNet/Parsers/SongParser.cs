using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.SongtekstenNet.Parsers
{
    internal interface ISongParser
    {
        Task<Subject> ParseSong(string url, string html);
   }

    internal class SongParser : ISongParser
    {
        public Task<Subject> ParseSong(string url, string html)
        {
            var splitted = url.Split('/');
            var artists = ExtractSongArtists(html).Select(a => a.ToDto());
            var song = new Song
            {
                Url = url,
                Id = Convert.ToInt32(splitted[splitted.Length - 3]),
                Lyrics = ExtractSongLyrics(html, true),
                Title = ExtractSongTitle(html),
                PrimaryArtist = artists.FirstOrDefault(),
                FeaturedArtists = artists.Skip(1).ToList()
            };
            return Task.FromResult<Subject>(song);
        }

        private string ExtractSongLyrics(string html, bool trimTrash)
        {
            var regex = new Regex(@"(?<=\<\/h1\>).*?(?=\<div)", RegexOptions.Singleline | RegexOptions.Multiline);
            var match = regex.Match(html);
            if (!match.Success) throw new Exception("No tag found");

            var whitespaces_stripped = match.Value.Replace("\r", "").Replace("\n", "").Replace("<br />", Environment.NewLine);

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

        private string ExtractSongTitle(string html)
        {
            var breadcrumbRegex = new Regex(@"(?<=\<ol class=\""breadcrumb\""\>).*?(?=\<\/ol\>)", RegexOptions.Singleline | RegexOptions.Multiline);
            var breadcrumbMatch = breadcrumbRegex.Match(html);
            if (!breadcrumbMatch.Success) throw new Exception("No breadcrumb found");

            var bc = breadcrumbMatch.Value;
            var titleregex = new Regex(@"(?<=\<li\>).*?(?=\<\/li\>)", RegexOptions.Singleline | RegexOptions.Multiline);
            var titleMatches = titleregex.Matches(bc);
            if (titleMatches.Count == 0) throw new Exception("No title found");

            return titleMatches[titleMatches.Count - 1].Value;
        }

        private List<Data.Artist> ExtractSongArtists(string html)
        {
            var regexUl = new Regex(@"\<h3\>Artiesten\<\/h3\>\s*\<ul.*?\>\s*(.*?)\s*\<\/ul\>");
            var matchUl = regexUl.Match(html);
            if (!matchUl.Success) throw new Exception("No UL found");

            var regexLi = new Regex(@"\<li.*?\>.*?\<a href\=\""(?<url>.*?)\""\>(?<name>.*?)\<\/a\>\<\/li\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var artist_matches = regexLi.Matches(matchUl.Value);
            var arr_matches = new Match[artist_matches.Count];
            artist_matches.CopyTo(arr_matches, 0);


            return arr_matches.Select(m => {
                var url_split = m.Groups["url"].Value.Split('/');
                return new Data.Artist
                {
                    Id = Convert.ToInt32(url_split[url_split.Length - 2]),
                    Url = m.Groups["url"].Value,
                    Name = m.Groups["name"].Value
                };
            }).ToList();
        }
    }
}
