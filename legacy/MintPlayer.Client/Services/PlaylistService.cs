using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MintPlayer.Client.Services
{
    public interface IPlaylistService
    {

    }
    internal class PlaylistService : IPlaylistService
    {
        private readonly HttpClient httpClient;
        public PlaylistService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
    }
}
