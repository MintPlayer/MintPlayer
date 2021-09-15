using MintPlayer.Fetcher.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius
{
    public class GeniusFetcher : Fetcher
    {
        private readonly HttpClient httpClient;
        public GeniusFetcher(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public override IEnumerable<Regex> UrlRegex
        {
            get
            {
                return new[] {
                    new Regex(@"https:\/\/genius\.com\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await SendRequest(httpClient, url);
			throw new NotImplementedException();
        }
    }
}
