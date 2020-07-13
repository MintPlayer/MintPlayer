using System.Threading.Tasks;
using Newtonsoft.Json;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.Genius.Parsers.V1
{
    internal interface IAlbumParser
    {
        Task<Subject> ParseAlbum(string pageData);
    }
    internal class AlbumParser : IAlbumParser
    {
        public Task<Subject> ParseAlbum(string pageData)
        {
            var data = JsonConvert.DeserializeObject<Data.AlbumData>(pageData);
            return Task.FromResult<Subject>(data.ToDto());
        }
    }
}
