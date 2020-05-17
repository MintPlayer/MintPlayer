using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MintPlayer.Client.Services
{
    public interface ISongService
    {

    }
    internal class SongService : ISongService
    {
        private readonly HttpClient httpClient;
        public SongService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
    }
}
