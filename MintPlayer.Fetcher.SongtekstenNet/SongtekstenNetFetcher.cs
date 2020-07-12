using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.SongtekstenNet.Parsers;

namespace MintPlayer.Fetcher.SongtekstenNet
{
    internal class SongtekstenNetFetcher : Fetcher
    {
        private readonly IRequestSender requestSender;
        private readonly IArtistParser artistParser;
        private readonly IAlbumParser albumParser;
        private readonly ISongParser songParser;
        public SongtekstenNetFetcher(IRequestSender requestSender, IArtistParser artistParser, IAlbumParser albumParser, ISongParser songParser)
        {
            this.requestSender = requestSender;
            this.artistParser = artistParser;
            this.albumParser = albumParser;
            this.songParser = songParser;
        }

        public override IEnumerable<Regex> UrlRegex
        {
            get
            {
                return new[] {
                    new Regex(@"https\:\/\/songteksten\.net\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await requestSender.SendRequest(url);
            var splitted = url.Split('/');

            if (url.StartsWith("https://songteksten.net/lyric/"))
            {
                return await songParser.ParseSong(url, html);
            }
            else if (url.StartsWith("https://songteksten.net/artist/"))
            {
                return await artistParser.ParseArtist(url, html);
            }
            else if (url.StartsWith("https://songteksten.net/albums/"))
            {
                return await albumParser.ParseAlbum(url, html);
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
