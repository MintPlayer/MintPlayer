using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Mappers
{
	internal class SongV2Mapper
	{
		public Task<Song> ToDto(Common.SongPreloadedState songData)
		{
			if (songData == null)
			{
				return null;
			}

			var primaryArtistId = Convert.ToInt64(songData.SongPage.TrackingData.First(td => td.Key == "Primary Artist ID").Value);
			var releaseDate = (string)songData.SongPage.TrackingData.First(td => td.Key == "Release Date").Value;
			var media = songData.Entities.SelectToken($"songs.{songData.SongPage.Id}.media").ToObject<Data.MediumData[]>()
				.Select(m =>
				{
					switch (m.Provider)
					{
						case "spotify":
							return new Medium { Type = MintPlayer.Fetcher.Abstractions.Enums.eMediumType.Spotify, Value = m.Url };
						case "soundcloud":
							return new Medium { Type = MintPlayer.Fetcher.Abstractions.Enums.eMediumType.SoundCloud, Value = m.Url };
						case "youtube":
							return new Medium { Type = MintPlayer.Fetcher.Abstractions.Enums.eMediumType.YouTube, Value = m.Url };
						default:
							return null;
					}
				}).Where(m => m != null);

			var result = new Song
			{
				Id = songData.SongPage.Id,
				Url = $"https://genius.com{songData.SongPage.Url}",
				Title = (string)songData.SongPage.TrackingData.First(td => td.Key == "Title").Value,
				ReleaseDate = DateTime.Parse(releaseDate),
				PrimaryArtist = new Artist
				{
					Id = primaryArtistId,
					Name = (string)songData.Entities.SelectToken($"artists.{primaryArtistId}.name"),
					Url = (string)songData.Entities.SelectToken($"artists.{primaryArtistId}.url"),
				},
				Media = media.ToList()
			};

			return Task.FromResult(result);
		}
	}
}
