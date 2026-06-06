using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MintPlayer.Client.Services
{
    public interface IMediumTypeService
    {

    }
    internal class MediumTypeService : IMediumTypeService
    {
        private readonly HttpClient httpClient;
        public MediumTypeService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
    }
}
