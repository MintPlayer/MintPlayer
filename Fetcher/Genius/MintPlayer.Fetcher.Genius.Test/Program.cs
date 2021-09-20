using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Abstractions;

namespace MintPlayer.Fetcher.Genius.Test
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var services = new ServiceCollection()
				.AddSingleton<HttpClient>()
				.AddFetcherContainer()
				.AddGeniusFetcher()
				.BuildServiceProvider();

			var fetcherContainer = services.GetService<IFetcherContainer>();
			//var url_genius_song = "https://genius.com/The-weeknd-i-feel-it-coming-lyrics";

			while (true)
			{
				var url_genius_song = Console.ReadLine();
				try
				{
					var genius_song = await fetcherContainer.Fetch(url_genius_song, true);
				}
				catch (Exception ex)
				{
					Console.Error.Write(ex.Message);
				}
			}
		}
	}
}
