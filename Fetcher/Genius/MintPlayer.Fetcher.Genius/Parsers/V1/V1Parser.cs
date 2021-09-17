using System;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Parsers;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Services;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1
{
	internal class V1Parser : IGeniusParser
	{
		private readonly IPageDataReader pageDataReader;
		private readonly IArtistV1Parser aristParser;
		private readonly IAlbumV1Parser albumParser;
		private readonly ISongV1Parser songParser;
		public V1Parser(IPageDataReader pageDataReader, IArtistV1Parser aristParser, IAlbumV1Parser albumParser, ISongV1Parser songParser)
		{
			this.pageDataReader = pageDataReader;
			this.aristParser = aristParser;
			this.albumParser = albumParser;
			this.songParser = songParser;
		}

		public bool IsMatch(string url, string html)
		{
			if (html.Contains(" itemprop=\"page_data\"></meta>"))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public async Task<Subject> Parse(string url, string html, bool trimTrash)
		{
			var pageDataText = await pageDataReader.ReadPageData(html);
			var pageData = JsonConvert.DeserializeObject<PageData.SubjectPageData>(pageDataText);
			switch (pageData.PageType)
			{
				case "song":
					var song = await songParser.Parse(html, pageDataText, trimTrash);
					song.Url = url;
					return song;
				case "profile":
					var artist = await aristParser.Parse(html, pageDataText);
					artist.Url = url;
					return artist;
				case "album":
					var album = await albumParser.Parse(html, pageDataText);
					album.Url = url;
					return album;
				default:
					throw new Exception("Invalid page_type");
			}
		}
	}
}
