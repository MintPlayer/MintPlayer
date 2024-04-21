using MintPlayer.Fetcher.Abstractions.Dtos;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V2.Parsers;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V2.Services;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2;

internal class V2Parser : IGeniusParser
{
	private readonly IPreloadedStateReader preloadedStateReader;
	private readonly IArtistV2Parser artistV2Parser;
	private readonly IAlbumV2Parser albumV2Parser;
	private readonly ISongV2Parser songV2Parser;

	public V2Parser(IPreloadedStateReader preloadedStateReader, IArtistV2Parser artistV2Parser, IAlbumV2Parser albumV2Parser, ISongV2Parser songV2Parser)
	{
		this.preloadedStateReader = preloadedStateReader;
		this.artistV2Parser = artistV2Parser;
		this.albumV2Parser = albumV2Parser;
		this.songV2Parser = songV2Parser;
	}

	public bool IsMatch(string url, string html)
		=> (html.Contains("<div id=\"application\">") & html.Contains("window.__PRELOADED_STATE__ = JSON.parse"));

	public async Task<Subject> Parse(string url, string html, bool trimTrash)
	{
		var preloadedStateText = await preloadedStateReader.ReadPreloadedState(html);
		var preloadedState = JsonConvert.DeserializeObject<Common.PreloadedState>(preloadedStateText);
		switch (preloadedState.CurrentPage)
		{
			case "songPage":
				var song = await songV2Parser.Parse(html, preloadedStateText, trimTrash);
				return song;
			default:
				throw new NotImplementedException();
		}
	}
}
