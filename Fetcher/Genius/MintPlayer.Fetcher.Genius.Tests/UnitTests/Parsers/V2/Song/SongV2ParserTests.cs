using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V2.Parsers;
using MintPlayer.Fetcher.Genius.Parsers.V2.Parsers;

namespace MintPlayer.Fetcher.Genius.Tests.UnitTests.Parsers.V2.Song
{
	[TestClass]
    public class SongV2ParserTests
    {
		private readonly IServiceProvider services;
		public SongV2ParserTests()
		{
			services = new ServiceCollection()
				// Unit to test
				.AddSingleton<ISongV2Parser, SongV2Parser>()
				// Don't mock the preloadedStateReader
				.AddSingleton<Abstractions.Parsers.V2.Services.IPreloadedStateReader, Genius.Parsers.V2.Services.PreloadedStateReader>()
				.AddSingleton<Genius.Parsers.V2.Mappers.SongV2Mapper>()
				.AddSingleton<Abstractions.Parsers.Helpers.ILyricsParser, Genius.Parsers.Helpers.LyricsParser>()
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task Parse()
		{
			var songV2Parser = services.GetService<ISongV2Parser>();
			var preloadedStateReader = services.GetService<Abstractions.Parsers.V2.Services.IPreloadedStateReader>();

			//using (var fs = new FileStream("Templates/V2/Song/Templates/sunset-jesus.html", FileMode.Open, FileAccess.Read))
			using (var fs = new FileStream("Templates/V2/Song/Templates/i-feel-it-coming.html", FileMode.Open, FileAccess.Read))
			using (var reader = new StreamReader(fs))
			{
				var html = await reader.ReadToEndAsync();
				var preloadedState = await preloadedStateReader.ReadPreloadedState(html);
				var song = await songV2Parser.Parse(html, preloadedState, true);

				Assert.IsNotNull(song.Lyrics);
			}
		}

	}
}
