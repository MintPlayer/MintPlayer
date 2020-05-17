using System;
using System.Net.Http;

namespace MintPlayer.Client.Services
{
    public interface IArtistService
    {

    }
    internal class ArtistService : IArtistService
    {
        private readonly HttpClient httpClient;
        public ArtistService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
    }
}
