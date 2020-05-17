using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MintPlayer.Client.Services
{
    public interface IAccountService
    {

    }
    internal class AccountService : IAccountService
    {
        private readonly HttpClient httpClient;
        public AccountService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
    }
}
