﻿namespace MintPlayer.Data.Entities
{
    internal class PlaylistSong
    {
        public PlaylistSong()
        {
        }

        public PlaylistSong(Playlist playlist, Song song)
        {
            Playlist = playlist;
            PlaylistId = playlist.Id;
            Song = song;
            SongId = song.Id;
        }

        public int PlaylistId { get; set; }
        public Playlist Playlist { get; set; }

        public int SongId { get; set; }
        public Song Song { get; set; }

        public int Index { get; set; }
    }
}
