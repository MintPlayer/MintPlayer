using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2
{
	internal class V2Parser : IGeniusParser
	{
		private readonly Services.IPreloadedStateReader preloadedStateReader;
		private readonly Artist.IArtistV2Parser artistV2Parser;
		private readonly Album.IAlbumV2Parser albumV2Parser;
		private readonly Abstractions.Parsers.V2.ISongV2Parser songV2Parser;

		public V2Parser(Services.IPreloadedStateReader preloadedStateReader, Artist.IArtistV2Parser artistV2Parser, Album.IAlbumV2Parser albumV2Parser, Abstractions.Parsers.V2.ISongV2Parser songV2Parser)
		{
			this.preloadedStateReader = preloadedStateReader;
			this.artistV2Parser = artistV2Parser;
			this.albumV2Parser = albumV2Parser;
			this.songV2Parser = songV2Parser;
		}

		public bool IsMatch(string url, string html)
		{
			if (html.Contains("<div id=\"application\">") & html.Contains("window.__PRELOADED_STATE__ = JSON.parse"))
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
			var preloadedStateText = await preloadedStateReader.ReadPreloadedState(html);
			var preloadedState = JsonConvert.DeserializeObject<Common.PreloadedState>(preloadedStateText);
			switch (preloadedState.CurrentPage)
			{
				case "songPage":
					var song = await songV2Parser.Parse(html, preloadedStateText);
					return song;
				default:
					throw new NotImplementedException();
					break;
			}
		}
	}
}
