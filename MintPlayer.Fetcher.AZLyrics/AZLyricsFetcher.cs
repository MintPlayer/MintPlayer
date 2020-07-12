using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.AZLyrics.Parsers;

namespace MintPlayer.Fetcher.AZLyrics
{
    internal class AZLyricsFetcher : Fetcher
    {
        private readonly IRequestSender requestSender;
        private readonly ISongParser songParser;
        private readonly IArtistParser artistParser;
        public AZLyricsFetcher(IRequestSender requestSender, ISongParser songParser, IArtistParser artistParser)
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
                    new Regex(@"https:\/\/www\.azlyrics\.com\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await requestSender.SendRequest(url);
            if (url.StartsWith("https://www.azlyrics.com/lyrics/"))
            {
                var song = await songParser.ParseSong(url, html, trimTrash);
                return song;
            }
            else if(Regex.IsMatch(url, @"https\:\/\/www\.azlyrics\.com\/[0-9a-z]+\/[0-9a-z]+\.html"))
            {
                var artist = await artistParser.ParseArtist(url, html);
                return artist;
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
