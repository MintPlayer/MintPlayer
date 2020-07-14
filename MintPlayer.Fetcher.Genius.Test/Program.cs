using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.DependencyInjection;
using MintPlayer.Fetcher.Genius.Parsers;
using System;
using System.IO;
using System.Net.Http;

namespace MintPlayer.Fetcher.Genius.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<HttpClient>()
                .AddFetcherContainer()
                .AddGeniusFetcher()
                .BuildServiceProvider();

            #region Test version 2
            var v2Html = File.ReadAllText("imtheone2.html");
            var song2 = serviceProvider.GetService<IV2Parser>()
                .Parse(v2Html, true)
                .Result;
            #endregion

            #region Test version 3
            var v3Html = File.ReadAllText("imtheone3.html");
            var song3 = serviceProvider.GetService<IV3Parser>()
                .Parse(v3Html, true)
                .Result;
            #endregion
        }
    }
}
