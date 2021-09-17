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

			//while (true)
			//{
			//	var url_genius_song = Console.ReadLine();
			//	try
			//	{
			//		var genius_song = await fetcherContainer.Fetch(url_genius_song, true);
			//	}
			//	catch (Exception ex)
			//	{
			//		Console.Error.Write(ex.Message);
			//	}
			//}

			//var songV1Parser = services.GetService<Abstractions.Parsers.V1.Parsers.ISongV1Parser>();
			//var pageDataReader = services.GetService<Abstractions.Parsers.V1.Services.IPageDataReader>();
			//using (var fs = new FileStream("Templates/V1/Song/i-feel-it-coming.html", FileMode.Open, FileAccess.Read))
			//using (var reader = new StreamReader(fs))
			//{
			//	var html = await reader.ReadToEndAsync();
			//	var pageData = await pageDataReader.ReadPageData(html);
			//	var song = await songV1Parser.Parse(html, pageData, true);
			//}

			//var songV2Parser = services.GetService<Abstractions.Parsers.V2.Parsers.ISongV2Parser>();
			//var preloadedStateReader = services.GetService<Abstractions.Parsers.V2.Services.IPreloadedStateReader>();
			//using (var fs = new FileStream("Templates/V2/Song/sunset-jesus.html", FileMode.Open, FileAccess.Read))
			//using (var reader = new StreamReader(fs))
			//{
			//	var html = await reader.ReadToEndAsync();
			//	var preloadedState = await preloadedStateReader.ReadPreloadedState(html);
			//	var song = await songV2Parser.Parse(html, preloadedState);
			//}
		}
	}
}
