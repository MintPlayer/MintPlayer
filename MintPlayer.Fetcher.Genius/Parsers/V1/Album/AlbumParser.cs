using System.Threading.Tasks;
using Newtonsoft.Json;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Album
{
    internal interface IAlbumParser
    {
        Task<Subject> ParseAlbum(string pageData);
    }
    internal class AlbumParser : IAlbumParser
    {
        public Task<Subject> ParseAlbum(string pageData)
        {
            var data = JsonConvert.DeserializeObject<AlbumData>(pageData);
            return Task.FromResult<Subject>(data.ToDto());
        }
    }
}
