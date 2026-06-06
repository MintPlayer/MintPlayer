using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MintPlayer.Client.Services
{
    public interface IPersonService
    {

    }
    internal class PersonService : IPersonService
    {
        private readonly HttpClient httpClient;
        public PersonService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
    }
}
