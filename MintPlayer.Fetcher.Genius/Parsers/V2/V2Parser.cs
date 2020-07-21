using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.Genius.Parsers.V2;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius.Parsers.V2
{
    internal class V2Parser : IGeniusVersionParser
    {
        private readonly Artist.IArtistParser artistParser;
        private readonly Album.IAlbumParser albumParser;
        private readonly Song.ISongParser songParser;
        private readonly ILdJsonReader ldJsonReader;
        public V2Parser(Artist.IArtistParser artistParser, Album.IAlbumParser albumParser, Song.ISongParser songParser, ILdJsonReader ldJsonReader)
        {
            this.artistParser = artistParser;
            this.albumParser = albumParser;
            this.songParser = songParser;
            this.ldJsonReader = ldJsonReader;
        }

        public Task<bool> IsMatch(string html)
        {
            var isMatch = html.Contains(@"<div id=""application"">");
            return Task.FromResult(isMatch);
        }

        public async Task<Subject> Parse(string html, bool trimTrash)
        {
            var ldJson = await ldJsonReader.ReadLdJson(html);
            var json = JsonConvert.DeserializeObject<Shared.Subject>(ldJson);
            switch (json.Type)
            {
                case "MusicRecording":
                    var song = await songParser.ParseSong(html, ldJson);
                    return song;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
