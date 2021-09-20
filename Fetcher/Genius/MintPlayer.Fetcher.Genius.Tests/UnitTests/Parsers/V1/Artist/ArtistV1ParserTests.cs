using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Parsers;
using MintPlayer.Fetcher.Genius.Parsers.V1.Parsers;

namespace MintPlayer.Fetcher.Genius.Tests.UnitTests.Parsers.V1.Artist
{
	[TestClass]
    public class ArtistV1ParserTests
    {
		private readonly IServiceProvider services;
		public ArtistV1ParserTests()
		{
			services = new ServiceCollection()
				// Unit to test
				.AddSingleton<IArtistV1Parser, ArtistV1Parser>()

				// Don't mock
				.AddSingleton<Abstractions.Parsers.V1.Services.IPageDataReader, Genius.Parsers.V1.Services.PageDataReader>()
				.AddSingleton<Genius.Parsers.V1.Mappers.ArtistV1Mapper>()
				.AddSingleton<Genius.Parsers.V1.Mappers.MediumV1Mapper>()
				.AddSingleton<Abstractions.Parsers.Helpers.ILdJsonReader, Genius.Parsers.Helpers.LdJsonReader>()

				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task Parse()
		{
			var artistV1Parser = services.GetService<IArtistV1Parser>();
			var pageDataReader = services.GetService<Abstractions.Parsers.V1.Services.IPageDataReader>();

			using (var fs = new FileStream("Templates/V1/Artist/the-weeknd.html", FileMode.Open, FileAccess.Read))
			using (var reader = new StreamReader(fs))
			{
				var html = await reader.ReadToEndAsync();
				var pageData = await pageDataReader.ReadPageData(html);
				var artist = await artistV1Parser.Parse(html, pageData);

				Assert.AreEqual("The Weeknd", artist.Name);
				Assert.AreEqual("https://genius.com/artists/The-weeknd", artist.Url);
				Assert.IsNotNull(artist.ImageUrl);
			}
		}

	}
}
