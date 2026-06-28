using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MintPlayer.Client.Services
{
    public interface ITagService
    {

    }
    internal class TagService : ITagService
    {
        private readonly HttpClient httpClient;
        public TagService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
    }
}
