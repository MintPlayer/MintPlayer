using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius
{
    public class GeniusFetcher : Fetcher
    {
        private readonly HttpClient httpClient;
		private readonly IServiceProvider serviceProvider;
		public GeniusFetcher(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            this.httpClient = httpClient;
			this.serviceProvider = serviceProvider;
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
			Console.WriteLine(html);
			var availableParsers = serviceProvider.GetServices<Parsers.IGeniusParser>();
			var parser = availableParsers.LastOrDefault(p => p.IsMatch(url, html));

			var subject = await parser.Parse(url, html, trimTrash);
			return subject;
        }
    }
}
