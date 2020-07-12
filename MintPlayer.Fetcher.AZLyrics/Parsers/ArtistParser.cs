using MintPlayer.Fetcher.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.AZLyrics.Parsers
{
    internal interface IArtistParser
    {
        Task<Subject> ParseArtist(string url, string html);
    }
    internal class ArtistParser : IArtistParser
    {
        public async Task<Subject> ParseArtist(string url, string html)
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
    }
}
