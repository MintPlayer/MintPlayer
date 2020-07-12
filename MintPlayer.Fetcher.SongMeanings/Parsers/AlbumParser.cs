using System;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Dtos;

namespace MintPlayer.Fetcher.SongMeanings.Parsers
{
    internal interface IAlbumParser
    {
        Task<Subject> ParseAlbum(string url, string html);
    }

    internal class AlbumParser : IAlbumParser
    {
        public Task<Subject> ParseAlbum(string url, string html)
        {
            throw new NotImplementedException();
        }
    }
}
