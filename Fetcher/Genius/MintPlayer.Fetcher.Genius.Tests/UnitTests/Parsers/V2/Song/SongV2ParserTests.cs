using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V2;
using MintPlayer.Fetcher.Genius.Parsers.V2.Song;

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
				// Don't mock the pageDataReader
				.AddSingleton<Abstractions.Parsers.V1.Services.IPageDataReader, Genius.Parsers.V1.Services.PageDataReader>()
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task Parse()
		{
			var songV2Parser = services.GetService<ISongV2Parser>();

			var pageDataReader = services.GetService<Abstractions.Parsers.V1.Services.IPageDataReader>();

			using (var fs = new FileStream("Templates/V1/Song/i-feel-it-coming.html", FileMode.Open, FileAccess.Read))
			using (var reader = new StreamReader(fs))
			{
				var html = await reader.ReadToEndAsync();
				var pageData = await pageDataReader.ReadPageData(html);
				var song = await songV2Parser.Parse(html, pageData);

				Assert.IsNotNull(song.Lyrics);
			}
		}

	}
}
