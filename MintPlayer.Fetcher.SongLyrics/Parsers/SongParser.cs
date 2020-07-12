using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.SongLyrics.Parsers
{
    internal interface ISongParser
    {
        Task<Subject> ParseSong(string url, string h1, string html);
    }

    internal class SongParser : ISongParser
    {
        public Task<Subject> ParseSong(string url, string h1, string html)
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
    }
}
