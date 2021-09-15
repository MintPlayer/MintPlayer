using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Crawler.DependencyInjection;
using MintPlayer.Fetcher.DependencyInjection;
using MintPlayer.Fetcher.Dtos;
using System.Collections.Generic;
using System.Net.Http;

namespace MintPlayer.Crawler.Test
{
    class Program
    {
        static List<Subject> subjects = new List<Subject>();

        static void Main(string[] args)
        {
            #region Register services
            var services = new ServiceCollection();
            services
                .AddSingleton<HttpClient>()
                .AddFetcherContainer()
                .AddGeniusFetcher()
                .AddMusixmatchFetcher()
                .AddLyricsComFetcher()
                .AddSongtekstenNetFetcher()
                .AddMintPlayerCrawler();
            #endregion

            var provider = services.BuildServiceProvider();

            #region Start crawler
            var crawler = provider.GetRequiredService<MintPlayerCrawler>();
                //.AddFetcher<Fetcher.Genius.GeniusFetcher>()
                //.AddFetcher<Fetcher.LyricsCom.LyricsComFetcher>()
                //.AddFetcher<Fetcher.Musixmatch.MusixmatchFetcher>()
                //.AddFetcher<Fetcher.SongtekstenNet.SongtekstenNetFetcher>();

            crawler.SubjectsDiscovered += (sender, e) =>
            {
                subjects.AddRange(e.Subjects);
            };

            crawler.Start().Wait();
            #endregion
        }
    }
}
