using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Services;

namespace MintPlayer.Fetcher.Genius.Tests.UnitTests.Parsers.V1.Services
{
	[TestClass]
    public class PageDataReaderTests
    {
		private readonly IServiceProvider services;
		public PageDataReaderTests()
		{
			services = new ServiceCollection()
				// Unit to test
				.AddSingleton<IPageDataReader, Genius.Parsers.V1.Services.PageDataReader>()
				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task ReadPageData()
		{
			var pageDataReader = services.GetService<IPageDataReader>();

			using (var fs = new FileStream("Templates/V1/Song/i-feel-it-coming.html", FileMode.Open, FileAccess.Read))
			using (var reader = new StreamReader(fs))
			{
				var html = await reader.ReadToEndAsync();
				var pageData = await pageDataReader.ReadPageData(html);

				Assert.IsNotNull(pageData);

				var json = Newtonsoft.Json.Linq.JObject.Parse(pageData);

				Assert.IsNotNull(json.SelectToken("song"));
				Assert.AreEqual("I Feel It Coming", json.SelectToken("song.title"));
				Assert.AreEqual("2016-11-18", json.SelectToken("song.release_date"));
				Assert.IsNotNull(json.SelectToken("lyrics_data.body.html"));
			}
		}

	}
}
