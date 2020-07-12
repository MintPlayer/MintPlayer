using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.Musixmatch.Parsers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Musixmatch
{
    internal class MusixmatchFetcher : Fetcher
    {
        private readonly IRequestSender requestSender;
        private readonly ISongParser songParser;
        private readonly IAlbumParser albumParser;
        private readonly IArtistParser artistParser;
        private readonly ILdJsonReader ldJsonReader;
        public MusixmatchFetcher(IRequestSender requestSender, ISongParser songParser, ILdJsonReader ldJsonReader, IAlbumParser albumParser, IArtistParser artistParser)
        {
            this.requestSender = requestSender;
            this.songParser = songParser;
            this.albumParser = albumParser;
            this.artistParser = artistParser;
            this.ldJsonReader = ldJsonReader;
        }

        public override IEnumerable<Regex> UrlRegex
        {
            get
            {
                return new[] {
                    new Regex(@"https\:\/\/(www\.){0,1}musixmatch.com\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await requestSender.SendRequest(url);
            if (url.StartsWith("https://www.musixmatch.com/lyrics/"))
            {
                var ld_json = await ldJsonReader.ReadLdJson(html);
                var song = await songParser.ParseSong(url, html, ld_json);
                return song;
            }
            else if (url.StartsWith("https://www.musixmatch.com/artist/"))
            {
                var artist = await artistParser.ParseArtist(url, html);
                return artist;
            }
            else if (url.StartsWith("https://www.musixmatch.com/album/"))
            {
                var album = await albumParser.ParseAlbum(url, html);
                return album;
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
