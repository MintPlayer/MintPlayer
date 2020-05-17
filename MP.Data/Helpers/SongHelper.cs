using System;
using System.Linq;
using System.Collections.Generic;
using MintPlayer.Data.Entities;

namespace MintPlayer.Data.Helpers
{
	internal class SongHelper
	{
		internal void CalculateUpdatedArtists(Song old, MintPlayer.Dtos.Dtos.Song @new, MintPlayerContext dbContext, out IEnumerable<ArtistSong> to_add, out IEnumerable<ArtistSong> to_update, out IEnumerable<ArtistSong> to_remove)
		{
			to_remove = old.Artists
				.Where(@as => !@new.Artists.Select(a => a.Id).Contains(@as.ArtistId))
				.ToArray();
			to_add = @new.Artists
				.Where(a => !old.Artists.Select(@as => @as.ArtistId).Contains(a.Id))
				.Select(a => new ArtistSong(dbContext.Artists.Find(a.Id), old))
				.ToArray();
			to_update = old.Artists
				.Except(to_remove)
				.ToArray();
		}
	}
}
