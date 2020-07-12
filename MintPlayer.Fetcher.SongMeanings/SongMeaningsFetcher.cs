using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.SongMeanings.Parsers;

namespace MintPlayer.Fetcher.SongMeanings
{
    internal class SongMeaningsFetcher : Fetcher
    {
        private readonly IRequestSender requestSender;
        private readonly IArtistParser artistParser;
        private readonly IAlbumParser albumParser;
        private readonly ISongParser songParser;
        public SongMeaningsFetcher(IRequestSender requestSender, IArtistParser artistParser, IAlbumParser albumParser, ISongParser songParser)
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
                return new[]
                {
                    new Regex(@"https\:\/\/(www\.){0,1}songmeanings\.com\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await requestSender.SendRequest(url);
            if (url.StartsWith("https://songmeanings.com/songs/view/"))
            {
                var song = await songParser.ParseSong(url, html);
                return song;
            }
            else if (url.StartsWith("https://songmeanings.com/artist/view/"))
            {
                var artist = await artistParser.ParseArtist(url, html);
                return artist;
            }
            else if (url.StartsWith("https://songmeanings.com/albums/view/tracks/"))
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
