using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Album
{
    internal class AlbumV1Parser : Abstractions.Parsers.V1.Album.IAlbumV1Parser
    {
        public Task<MintPlayer.Fetcher.Abstractions.Dtos.Album> Parse(string html, string pageData)
        {
            throw new System.NotImplementedException();
        }
    }
}
