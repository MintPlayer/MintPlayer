﻿using Microsoft.Extensions.DependencyInjection;
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

            #region Version 1

            #region Song

            using (var fs = new FileStream("Parsers/V1/Song/Example.html", FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fs))
            {
                var songV1 = reader.ReadToEnd();
                serviceProvider
                    .GetService<Parsers.V1.Song.ISongParser>()
                    .Parse(songV1, true);
            }

            #endregion

            #endregion

            //const string url = "https://genius.com/artists/Avicii";
            //var avicii = serviceProvider.GetService<IFetcherContainer>()
            //    .Fetch(url, true)
            //    .Result;

            //#region Test version 2
            //var v2Html = File.ReadAllText("imtheone2.html");
            //var song2 = serviceProvider.GetService<Parsers.V2.IV2Parser>()
            //    .Parse(v2Html, true)
            //    .Result;
            //#endregion

            //#region Test version 3
            //var v3Html = File.ReadAllText("imtheone3.html");
            //var song3 = serviceProvider.GetService<Parsers.V3.IV3Parser>()
            //    .Parse(v3Html, true)
            //    .Result;
            //#endregion
        }
    }
}