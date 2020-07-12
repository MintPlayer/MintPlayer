using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.LoloLyrics.Parsers
{
    internal interface IArtistParser
    {
        Task<Subject> ParseArtist(string url, string html);
    }

    internal class ArtistParser : IArtistParser
    {
        public Task<Subject> ParseArtist(string url, string html)
        {
            var result = new Artist
            {
                Url = url,
                Name = ExtractArtistName(html),
                Songs = ExtractArtistSongs(html)
            };
            return Task.FromResult<Subject>(result);
        }

        private string ExtractArtistName(string html)
        {
            var rgx = new Regex(@"\<span class\=\""head\""\>Lyrics by (?<artist>.*?)\<\/span\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (!m.Success) throw new Exception("Could not extract artist name");

            return m.Groups["artist"].Value;
        }
        private List<Song> ExtractArtistSongs(string html)
        {
            var rgxSong = new Regex(@"\<div class\=\""list_item\""\>\s*\<div class\=\""cover\""\>\s*\<a href\=\""(?<url>.*?)\""\>\s*\<img alt\=\""Cover\: (?<artist>.*?) \- (?<title>.*?)\"" src\=\""(?<image>.*?)\"" \/\>\s*\<\/a\>\s*\<\/div\>\s*\<div class\=\""info\""\>\s*\<span.*?\>\s*\<\/span\>\s*\<span.*?\>\s*\<a href\=\""(?<url>.*?)\"".*?\>\k<title>\<\/a\>\s*\<\/span\>.*?\<\/div\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m_songs = rgxSong.Matches(html);
            var arr_songs = new Match[m_songs.Count];
            m_songs.CopyTo(arr_songs, 0);

            return arr_songs.Select(m => new Song
            {
                Url = "https://www.lololyrics.com" + m.Groups["url"].Value,
                Title = m.Groups["title"].Value,
            }).ToList();
        }
    }
}
