using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.Genius.Parsers.V1;
using MintPlayer.Fetcher.Genius.Parsers.V1.Helpers;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius.Parsers.V1
{
    internal class V1Parser : IGeniusVersionParser
    {
        private readonly Artist.IArtistParser artistParser;
        private readonly Album.IAlbumParser albumParser;
        private readonly Song.ISongParser songParser;
        private readonly IPageDataReader pageDataReader;
        public V1Parser(Artist.IArtistParser artistParser, Album.IAlbumParser albumParser, Song.ISongParser songParser, IPageDataReader pageDataReader)
        {
            this.artistParser = artistParser;
            this.albumParser = albumParser;
            this.songParser = songParser;
            this.pageDataReader = pageDataReader;
        }
        
        public Task<bool> IsMatch(string html)
        {
            var isMatch = html.Contains("__PRELOADED_STATE__");
            return Task.FromResult(isMatch);
        }

        public async Task<Subject> Parse(string html, bool trimTrash)
        {
            var page_data = await pageDataReader.Read(html);

            var structure = new { currentPage = string.Empty };
            var subject = JsonConvert.DeserializeAnonymousType(page_data, structure);

            switch (subject.currentPage)
            {
                case "profile":
                    return await artistParser.ParseArtist(page_data);
                case "songPage":
                    return await songParser.Parse(html, trimTrash);
                case "album":
                    return await albumParser.ParseAlbum(page_data);
                default:
                    throw new Exception("Type not recognized");
            }
        }
    }
}
