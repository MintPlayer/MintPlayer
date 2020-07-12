using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.LyricsCom.Parsers;

namespace MintPlayer.Fetcher.LyricsCom
{
    internal class LyricsComFetcher : Fetcher
    {
        private readonly IRequestSender requestSender;
        private readonly ISongParser songParser;
        private readonly IAlbumParser albumParser;
        private readonly IArtistParser artistParser;
        public LyricsComFetcher(IRequestSender requestSender, ISongParser songParser, IAlbumParser albumParser, IArtistParser artistParser)
        {
            this.requestSender = requestSender;
            this.songParser = songParser;
            this.albumParser = albumParser;
            this.artistParser = artistParser;
        }

        public override IEnumerable<Regex> UrlRegex
        {
            get
            {
                return new[]
                {
                    new Regex(@"https\:\/\/www\.lyrics\.com\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await requestSender.SendRequest(url);
            if (url.StartsWith("https://www.lyrics.com/lyric/"))
            {
                var song = await songParser.ParseSong(url, html);
                return song;
            }
            else if (url.StartsWith("https://www.lyrics.com/artist/"))
            {
                var artist = await artistParser.ParseArtist(url, html);
                return artist;
            }
            else if (url.StartsWith("https://www.lyrics.com/album/"))
            {
                var album = await albumParser.ParseAlbum(url, html);
                return album;
            }
            else
            {
                throw new Exception("URL cannot be mapped");
            }
        }
    }
}
