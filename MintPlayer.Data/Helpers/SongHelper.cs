using MintPlayer.Data.Entities;
using System.Text.RegularExpressions;

namespace MintPlayer.Data.Helpers;

internal class SongHelper
{
	internal void CalculateUpdatedArtists(Song old, MintPlayer.Dtos.Dtos.Song @new, MintPlayerContext dbContext, out IEnumerable<ArtistSong> to_add, out IEnumerable<ArtistSong> to_update, out IEnumerable<ArtistSong> to_remove)
	{
		// Compute artists to remove
		var en_to_remove = old.Artists.Where(@as =>
		{
			if (@new.Artists.Any(a => a.Id == @as.ArtistId)) return false;
			if (@new.UncreditedArtists.Any(a => a.Id == @as.ArtistId)) return false;
			return true;
		});

		// Compute artists to add
		var en_to_add = @new.Artists
			.Select(a => new { ArtistId = a.Id, Credited = true })
			.Concat(@new.UncreditedArtists.Select(a => new { ArtistId = a.Id, Credited = false }))
			.Where(a => !old.Artists.Any(@as => @as.ArtistId == a.ArtistId))
			.Select(a => new ArtistSong
			{
				Artist = dbContext.Artists.Find(a.ArtistId),
				ArtistId = a.ArtistId,
				Song = old,
				SongId = old.Id,
				Credited = a.Credited
			});

		// Compute artists to update
		var en_to_update = old.Artists.Except(en_to_remove);
		foreach (var item in en_to_update)
		{
			if (@new.Artists.Any(a => a.Id == item.ArtistId))
				item.Credited = true;
			else if (@new.UncreditedArtists.Any(a => a.Id == item.ArtistId))
				item.Credited = false;
			else
				throw new Exception("Not supposed to happen");
		}

		to_remove = en_to_remove.ToArray();
		to_add = en_to_add.ToArray();
		to_update = en_to_update.ToArray();
	}

	internal IEnumerable<MintPlayer.Dtos.Dtos.PlayerInfo> GetPlayerInfos(IEnumerable<Medium> media)
	{
		if (media != null)
		{
			var playableRegexes = new Dictionary<Regex, MintPlayer.Dtos.Enums.EPlayerType>
			{
				[new Regex(@"http[s]{0,1}:\/\/(www\.){0,1}youtube\.com\/watch\?v=(?<id>[^&]+)")] = MintPlayer.Dtos.Enums.EPlayerType.Youtube,
				[new Regex(@"http[s]{0,1}:\/\/m\.youtube\.com\/watch\?v=(?<id>[^&]+)")] = MintPlayer.Dtos.Enums.EPlayerType.Youtube,
				[new Regex(@"http[s]{0,1}:\/\/(www\.){0,1}youtu\.be\/(?<id>.+)$")] = MintPlayer.Dtos.Enums.EPlayerType.Youtube,

				[new Regex(@"http[s]{0,1}:\/\/(www\.){0,1}dailymotion\.com\/video\/(?<id>[0-9A-Za-z]+)$")] = MintPlayer.Dtos.Enums.EPlayerType.DailyMotion,

				[new Regex(@"http[s]{0,1}:\/\/(www\.){0,1}vimeo\.com\/(?<id>[0-9]+)$")] = MintPlayer.Dtos.Enums.EPlayerType.Vimeo,

				[new Regex(@"(?<id>http[s]{0,1}:\/\/(www\.){0,1}soundcloud\.com\/.+)$")] = MintPlayer.Dtos.Enums.EPlayerType.SoundCloud
			};

			foreach (var medium in media)
			{
				foreach (var regex in playableRegexes)
				{
					var match = regex.Key.Match(medium.Value);
					if (match.Success)
					{
						var id = match.Groups["id"].Value;
						yield return new MintPlayer.Dtos.Dtos.PlayerInfo
						{
							Type = regex.Value,
							Url = medium.Value,
							Id = id,
							ImageUrl = regex.Value switch
							{
								MintPlayer.Dtos.Enums.EPlayerType.Youtube => $"https://i.ytimg.com/vi/{id}/hqdefault.jpg",
								MintPlayer.Dtos.Enums.EPlayerType.DailyMotion => $"https://www.dailymotion.com/thumbnail/video/{id}",
								MintPlayer.Dtos.Enums.EPlayerType.Vimeo => $"https://i.vimeocdn.com/video/99213072?mw=960&mh=540",
								MintPlayer.Dtos.Enums.EPlayerType.SoundCloud => $"https://i1.sndcdn.com/artworks-eA5afLkRFfiD-0-t500x500.jpg",
								_ => null
							}
						};
					}
				}
			}
		}
	}
}
