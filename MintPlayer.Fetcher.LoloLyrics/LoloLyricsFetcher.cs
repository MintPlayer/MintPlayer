using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.LoloLyrics.Parsers;

namespace MintPlayer.Fetcher.LoloLyrics
{
    internal class LoloLyricsFetcher : Fetcher
    {
        private readonly IRequestSender requestSender;
        private readonly ISongParser songParser;
        private readonly IArtistParser artistParser;
        public LoloLyricsFetcher(IRequestSender requestSender, ISongParser songParser, IArtistParser artistParser)
        {
            this.requestSender = requestSender;
            this.songParser = songParser;
            this.artistParser = artistParser;
        }
        public override IEnumerable<Regex> UrlRegex
        {
            get
            {
                return new[]
                {
                    new Regex(@"https\:\/\/www\.lololyrics\.com\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await requestSender.SendRequest(url);
            if (url.StartsWith("https://www.lololyrics.com/artist/"))
            {
                var artist = await artistParser.ParseArtist(url, html);
                return artist;
            }
            else if(url.StartsWith("https://www.lololyrics.com/lyrics/"))
            {
                var song = await songParser.ParseSong(url, html);
                return song;
            }
            else
            {
                throw new Exception("URL cannot be mapped");
            }
        }
    }
}
