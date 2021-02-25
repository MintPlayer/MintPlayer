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
	}
}
