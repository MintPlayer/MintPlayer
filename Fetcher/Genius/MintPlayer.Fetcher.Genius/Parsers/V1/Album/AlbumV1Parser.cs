using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Album
{
    internal interface IAlbumV1Parser
    {
        Task<Subject> Parse(string html, string pageData);
    }
    internal class AlbumV1Parser : IAlbumV1Parser
    {
        public Task<Subject> Parse(string html, string pageData)
        {
            throw new System.NotImplementedException();
        }
    }
}
