using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.SongLyrics.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.SongLyrics
{
    internal class SongLyricsFetcher : Fetcher
    {
        private readonly IRequestSender requestSender;
        private readonly ISongParser songParser;
        private readonly IArtistParser artistParser;
        private readonly IAlbumParser albumParser;
        public SongLyricsFetcher(IRequestSender requestSender, ISongParser songParser, IArtistParser artistParser, IAlbumParser albumParser)
        {
            this.requestSender = requestSender;
            this.songParser = songParser;
            this.artistParser = artistParser;
            this.albumParser = albumParser;
        }
        public override IEnumerable<Regex> UrlRegex
        {
            get
            {
                return new[] {
                    new Regex(@"http:\/\/www\.songlyrics\.com\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await requestSender.SendRequest(url);
            var h1 = ReadH1(html);
            if (IsArtist(h1, html))
            {
                var artist = await artistParser.ParseArtist(url, h1, html);
                return artist;
            }
            else if (IsSong(h1, html))
            {
                var song = await songParser.ParseSong(url, h1, html);
                return song;
            }
            else if (IsAlbum(h1, html))
            {
                var album = await albumParser.ParseAlbum(url, h1, html);
                return album;
            }
            else
            {
                throw new Exception("Could not determine subject type");
            }
        }

        #region Helper methods
        private string ReadH1(string html)
        {
            var rgx = new Regex(@"\<h1\>(?<title>.*)\<\/h1\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var m = rgx.Match(html);
            if (!m.Success) throw new Exception("No h1 tag found");

            return m.Groups["title"].Value;
        }
        #endregion
        #region Distinct methods
        private bool IsAlbum(string h1, string html)
        {
            return h1.EndsWith(" Album");
        }
        private bool IsSong(string h1, string html)
        {
            var rgxArtist = new Regex(@"\<p\>Artist\: \<a href\=\"".*?\"" title\=\"".*?\""\>.*?\<\/a\>\<\/p\>", RegexOptions.Singleline);
            return h1.EndsWith(" Lyrics") & rgxArtist.IsMatch(html);
        }
        private bool IsArtist(string h1, string html)
        {
            var rgxArtist = new Regex(@"\<p\>Artist\: \<a href\=\"".*?\"" title\=\"".*?\""\>.*?\<\/a\>\<\/p\>", RegexOptions.Singleline);
            return h1.EndsWith(" Song Lyrics") & !rgxArtist.IsMatch(html);
        }
        #endregion
    }
}
