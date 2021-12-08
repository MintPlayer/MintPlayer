using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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

		public Guid UserId { get; set; }
		public User User { get; set; }

		public string Text { get; set; }
		public List<double> Timeline { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
