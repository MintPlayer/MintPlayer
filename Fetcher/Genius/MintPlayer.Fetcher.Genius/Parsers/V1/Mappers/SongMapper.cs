using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Mappers
{
	internal class SongMapper
	{
		private readonly ArtistMapper artistMapper;
		public SongMapper(ArtistMapper artistMapper)
		{
			this.artistMapper = artistMapper;
		}

		public async Task<Song> ToDto(Data.SongData song)
		{
			if (song == null)
			{
				return null;
			}

			var result = new Song
			{
				Id = song.Id,
				Url = song.Url,
				Title = song.Title,
				ReleaseDate = song.ReleaseDate,
				PrimaryArtist = await artistMapper.ToDto(song.PrimaryArtist),
			};

			if (song.FeaturedArtists != null)
			{
				var artists = await Task.WhenAll(song.FeaturedArtists.Select(a => artistMapper.ToDto(a)));
				result.FeaturedArtists = artists.ToList();
			}

			return result;
		}

		public async Task<Song> ToDto(Data.SongData song, PageData.LyricsData lyrics, bool trimTrash)
		{
			var result = await ToDto(song);

			if (result == null)
			{
				return null;
			}

			if (lyrics != null)
			{
				result.Lyrics = await ParseLyrics(lyrics.Body.Html, trimTrash);
			}

			return result;
		}

		private Task<string> ParseLyrics(string lyricsHtml, bool trimTrash)
		{
			var rgx = new Regex(@"\<a [\s\S]*?\>|\<\/a\>|\<p\>|\<\/p\>", RegexOptions.Multiline);
			var stripped = rgx.Replace(lyricsHtml, string.Empty)
				.Replace("\r", string.Empty)
				.Replace("\n", string.Empty)
				.Replace("<br>", Environment.NewLine);

			if (trimTrash)
			{
				var rgxBrackets = new Regex(@"\[.*?\]\r\n", RegexOptions.Multiline);
				stripped = rgxBrackets.Replace(stripped, string.Empty);
			}

			return Task.FromResult(stripped);
		}
	}
}
