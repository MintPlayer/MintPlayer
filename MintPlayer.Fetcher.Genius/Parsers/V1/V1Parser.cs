using System;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Dtos;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V1
{
	internal class V1Parser : IGeniusParser
	{
		private readonly Services.IPageDataReader pageDataReader;
		private readonly Artist.IArtistV1Parser aristParser;
		private readonly Album.IAlbumV1Parser albumParser;
		private readonly Song.ISongV1Parser songParser;
		public V1Parser(Services.IPageDataReader pageDataReader, Artist.IArtistV1Parser aristParser, Album.IAlbumV1Parser albumParser, Song.ISongV1Parser songParser)
		{
			this.pageDataReader = pageDataReader;
			this.aristParser = aristParser;
			this.albumParser = albumParser;
			this.songParser = songParser;
		}

		public bool IsMatch(string url, string html)
		{
			return true;
		}

		public async Task<Subject> Parse(string url, string html, bool trimTrash)
		{
			var pageDataText = await pageDataReader.ReadPageData(html);
			var pageData = JsonConvert.DeserializeObject<Common.SubjectPageData>(pageDataText);
			switch (pageData.PageType)
			{
				case "song":
					var song = await songParser.Parse(html, pageDataText);
					return song;
				case "profile":
					var artist = await aristParser.Parse(html, pageDataText);
					return artist;
				case "album":
					var album = await albumParser.Parse(html, pageDataText);
					return album;
				default:
					throw new Exception("Invalid page_type");
			}
		}
	}
}
