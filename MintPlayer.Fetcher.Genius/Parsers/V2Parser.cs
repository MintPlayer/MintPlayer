using MintPlayer.Fetcher.Dtos;
using MintPlayer.Fetcher.Genius.Parsers.V2;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius.Parsers
{
    internal interface IV2Parser
    {
        Task<Subject> Parse(string html, bool trimTrash);
    }
    internal class V2Parser : IV2Parser
    {
        private readonly IArtistParser artistParser;
        private readonly IAlbumParser albumParser;
        private readonly ISongParser songParser;
        private readonly ILdJsonReader ldJsonReader;
        public V2Parser(IArtistParser artistParser, IAlbumParser albumParser, ISongParser songParser, ILdJsonReader ldJsonReader)
        {
            this.artistParser = artistParser;
            this.albumParser = albumParser;
            this.songParser = songParser;
            this.ldJsonReader = ldJsonReader;
        }

        public async Task<Subject> Parse(string html, bool trimTrash)
        {
            var ldJson = await ldJsonReader.ReadLdJson(html);
            var json = JsonConvert.DeserializeObject<V2.Data.Subject>(ldJson);
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
