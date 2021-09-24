using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V2.Parsers;
using MintPlayer.Fetcher.Genius.Parsers.V2.Mappers;
using Newtonsoft.Json;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Parsers
{
	internal class SongV2Parser : ISongV2Parser
	{
		private readonly SongV2Mapper songV2Mapper;
		public SongV2Parser(SongV2Mapper songV2Mapper)
		{
			this.songV2Mapper = songV2Mapper;
		}

		public async Task<Song> Parse(string html, string preloadedState, bool trimTrash)
		{
			var songPreloadedState = JsonConvert.DeserializeObject<Common.SongPreloadedState>(preloadedState);
			var song = await songV2Mapper.ToDto(songPreloadedState, trimTrash);
			try { song.FeaturedArtists = ExtractFeaturedArtists(html).ToList(); }
			catch (Exceptions.NoFeaturingArtistsFoundException) { }

			return song;
		}

		private IEnumerable<Artist> ExtractFeaturedArtists(string html)
		{
			var rgx = new Regex(@"\<div class\=\""HeaderMetadata__Section\-.*?\""\>\<p class\=\""HeaderMetadata__Label\-.*?\""\>Featuring\<\/p\>(?<artists>.*?)\<\/div\>");
			var m = rgx.Match(html);
			if (!m.Success)
			{
				throw new Exceptions.NoFeaturingArtistsFoundException("Could not find a Featuring tag");
			}

			var rgxArtists = new Regex(@"\<a href\=\""(?<url>.*?)\"" .*?\>(?<name>.*?)\<\/a\>");
			var artistMatches = rgxArtists.Matches(m.Groups["artists"].Value);
			var artistMatchesArray = new Match[artistMatches.Count];
			artistMatches.CopyTo(artistMatchesArray, 0);

			return artistMatchesArray
				.Select(m_artist => new Artist
				{
					Name = m_artist.Groups["name"].Value,
					Url = m_artist.Groups["url"].Value
				});
		}
	}
}
