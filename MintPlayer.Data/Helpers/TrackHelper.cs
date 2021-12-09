using System.Linq;
using System.Collections.Generic;
using MintPlayer.Data.Entities;

namespace MintPlayer.Data.Helpers
{
    internal class TrackHelper
    {
        internal void CalculateUpdatedTracks(Playlist old, MintPlayer.Dtos.Dtos.Playlist @new, MintPlayerContext mintplayer_context, out IEnumerable<PlaylistSong> to_add, out IEnumerable<PlaylistSong> to_update, out IEnumerable<PlaylistSong> to_remove)
        {
            // Compute tracks to remove
            var en_to_remove = old.Tracks.Where(track =>
            {
                return !@new.Tracks.Any(song => song.Id == track.SongId);
            });

            // Compute tracks to add
            var en_to_add = @new.Tracks
                .Select((song, index) => new { Index = index, Song = song })
                .Where(song => !old.Tracks.Any(track => track.SongId == song.Song.Id))
                .Select(song => new PlaylistSong
                {
                    Playlist = old,
                    PlaylistId = old.Id,
                    Song = mintplayer_context.Songs.Find(song.Song.Id),
                    SongId = song.Song.Id,
                    Index = song.Index
                });

            // Compute tracks to update
            var en_to_update = old.Tracks.Except(en_to_remove);
            foreach (var item in en_to_update)
            {
                var song = @new.Tracks.FirstOrDefault(song => song.Id == item.SongId);
                item.Index = @new.Tracks.IndexOf(song);
            }

            // Yield the results already
            // In the calling method the DbSets are changing. Here we have to evaluate the Linq expressions already.
            to_remove = en_to_remove.ToArray();
            to_add = en_to_add.ToArray();
            to_update = en_to_update.ToArray();
        }
    }
}
