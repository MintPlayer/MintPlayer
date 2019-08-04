using System;

namespace MintPlayer.Data.Entities
{

    internal class Lyrics
    {
        public Lyrics()
        {
        }
        public Lyrics(User user, Song song) : this()
        {
            User = user;
            Song = song;
        }

        public int SongId { get; set; }
        public Song Song { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string Text { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
