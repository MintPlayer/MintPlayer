using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MintPlayer.Client.Services
{
    public interface ISubjectService
    {

    }
    internal class SubjectService : ISubjectService
    {
        private readonly HttpClient httpClient;
        public SubjectService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
    }
}
