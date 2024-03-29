﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Parsers;
using MintPlayer.Fetcher.Genius.Parsers.V1.Parsers;

namespace MintPlayer.Fetcher.Genius.Tests.UnitTests.Parsers.V1.Song
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

				// Don't mock
				.AddSingleton<Abstractions.Parsers.V1.Services.IPageDataReader, Genius.Parsers.V1.Services.PageDataReader>()
				.AddSingleton<Genius.Parsers.V1.Mappers.SongV1Mapper>()
				.AddSingleton<Genius.Parsers.V1.Mappers.ArtistV1Mapper>()
				.AddSingleton<Genius.Parsers.V1.Mappers.MediumV1Mapper>()
				.AddSingleton<Abstractions.Parsers.Helpers.ILyricsParser, Genius.Parsers.Helpers.LyricsParser>()

				.BuildServiceProvider();
		}

		[TestMethod]
		public async Task IFeelItComing()
		{
			var songV1Parser = services.GetService<ISongV1Parser>();

			var pageDataReader = services.GetService<Abstractions.Parsers.V1.Services.IPageDataReader>();

			using (var fs = new FileStream("Templates/V1/Song/i-feel-it-coming.html", FileMode.Open, FileAccess.Read))
			using (var reader = new StreamReader(fs))
			{
				var html = await reader.ReadToEndAsync();
				var pageData = await pageDataReader.ReadPageData(html);
				var song = await songV1Parser.Parse(html, pageData, true);

				Assert.AreEqual("I Feel It Coming", song.Title);
				Assert.AreEqual(new DateTime(2016, 11, 18), song.ReleaseDate);
				Assert.IsNotNull(song.Lyrics);
				Assert.IsNotNull(song.PrimaryArtist);
				Assert.IsNotNull(song.FeaturedArtists);
				Assert.AreEqual(1, song.FeaturedArtists.Count);
			}
		}
		
		[TestMethod]
		public async Task Mia()
		{
			var songV1Parser = services.GetService<ISongV1Parser>();

			var pageDataReader = services.GetService<Abstractions.Parsers.V1.Services.IPageDataReader>();

			using (var fs = new FileStream("Templates/V1/Song/mia.html", FileMode.Open, FileAccess.Read))
			using (var reader = new StreamReader(fs))
			{
				var html = await reader.ReadToEndAsync();
				var pageData = await pageDataReader.ReadPageData(html);
				var song = await songV1Parser.Parse(html, pageData, true);

				Assert.AreEqual("Mia", song.Title);
				Assert.AreEqual(null, song.ReleaseDate);
				Assert.IsNotNull(song.Lyrics);
				Assert.IsNotNull(song.PrimaryArtist);
				Assert.IsNotNull(song.FeaturedArtists);
				Assert.AreEqual(0, song.FeaturedArtists.Count);
			}
		}

	}
}
