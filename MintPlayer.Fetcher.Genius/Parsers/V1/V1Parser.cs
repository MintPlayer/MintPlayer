using System;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius.Parsers.V1
{
	internal class V1Parser : IGeniusParser
	{
		public V1Parser(Artist.IArtistV1Parser aristParser, Album.IAlbumV1Parser albumParser, Song.ISongV1Parser songParser)
		{
		}

		public Task<bool> IsMatch(string html)
		{
			throw new NotImplementedException();
		}
	}
}
