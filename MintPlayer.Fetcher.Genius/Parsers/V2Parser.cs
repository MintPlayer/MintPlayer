using MintPlayer.Fetcher.Genius.Parsers.V2;

namespace MintPlayer.Fetcher.Genius.Parsers
{
    internal interface IV2Parser
    {

    }
    internal class V2Parser : IV2Parser
    {
        private readonly IArtistParser artistParser;
        private readonly IAlbumParser albumParser;
        private readonly ISongParser songParser;
        public V2Parser(IArtistParser artistParser, IAlbumParser albumParser, ISongParser songParser)
        {
            this.artistParser = artistParser;
            this.albumParser = albumParser;
            this.songParser = songParser;
        }
    }
}
