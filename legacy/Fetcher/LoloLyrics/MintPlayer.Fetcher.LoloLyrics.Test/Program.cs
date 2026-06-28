using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MintPlayer.Fetcher.LoloLyrics.Test
{
    class Program
    {
        static void Main(string[] args)
		{
			var services = new ServiceCollection()
				.AddSingleton<HttpClient>()
				.AddFetcherContainer()
				.AddLoloLyricsFetcher()
				.BuildServiceProvider();
		}
    }
}
