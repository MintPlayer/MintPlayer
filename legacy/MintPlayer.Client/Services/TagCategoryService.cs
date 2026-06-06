using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MintPlayer.Client.Services
{
    public interface ITagCategoryService
    {

    }
    internal class TagCategoryService : ITagCategoryService
    {
        private readonly HttpClient httpClient;
        public TagCategoryService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
    }
}
