using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.Helpers;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Mappers
{
	internal class SongV1Mapper
	{
		private readonly ArtistV1Mapper artistMapper;
		private readonly MediumV1Mapper mediumMapper;
		private readonly ILyricsParser lyricsParser;
		public SongV1Mapper(ArtistV1Mapper artistMapper, MediumV1Mapper mediumMapper, ILyricsParser lyricsParser)
		{
			this.artistMapper = artistMapper;
			this.mediumMapper = mediumMapper;
			this.lyricsParser = lyricsParser;
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

			if (song.Media != null)
			{
				var media = await Task.WhenAll(song.Media.Select(m => mediumMapper.ToDto(m)));
				result.Media = media.ToList();
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
				result.Lyrics = await lyricsParser.ParseLyrics(lyrics.Body.Html, trimTrash);
			}

			if (song.Media != null)
			{
				var media = await Task.WhenAll(song.Media.Select(m => mediumMapper.ToDto(m)));
				result.Media = media.ToList();
			}

			return result;
		}
	}
}
