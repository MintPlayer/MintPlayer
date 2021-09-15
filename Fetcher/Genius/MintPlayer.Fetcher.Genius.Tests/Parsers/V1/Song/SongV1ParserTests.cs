using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Song;
using MintPlayer.Fetcher.Genius.Parsers.V1.Song;

namespace MintPlayer.Fetcher.Genius.Tests.Parsers.V1.Song
{
	[TestClass]
    public class SongV1ParserTests
    {
		private readonly IServiceProvider services;
		public SongV1ParserTests()
		{
			services = new ServiceCollection()
				// Unit to test
				.AddSingleton<ISongV1Parser, SongV1Parser>()
				// Don't mock the pageDataReader
				.AddSingleton<Abstractions.Parsers.V1.Services.IPageDataReader, Genius.Parsers.V1.Services.PageDataReader>()
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task Parse()
		{
			var songV1Parser = services.GetService<ISongV1Parser>();

			var pageDataReader = services.GetService<Abstractions.Parsers.V1.Services.IPageDataReader>();

			using (var fs = new FileStream("Templates/V1/Song/i-feel-it-coming.html", FileMode.Open, FileAccess.Read))
			using (var reader = new StreamReader(fs))
			{
				var html = await reader.ReadToEndAsync();
				var pageData = await pageDataReader.ReadPageData(html);
				var song = await songV1Parser.Parse(html, pageData, true);
			}
		}

	}
}
