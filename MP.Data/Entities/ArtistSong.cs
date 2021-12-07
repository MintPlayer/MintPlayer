using System.ComponentModel.DataAnnotations.Schema;

namespace MintPlayer.Data.Entities
{
	[Table("ArtistSong", Schema = "mintplay")]
	internal class ArtistSong
	{
		public ArtistSong()
		{
		}
		public ArtistSong(Artist artist, Song song)
		{
			Artist = artist;
			ArtistId = artist?.Id ?? 0;
			Song = song;
			SongId = song?.Id ?? 0;
		}

		public int ArtistId { get; set; }
		public Artist Artist { get; set; }

		public int SongId { get; set; }
		public Song Song { get; set; }

        public bool Credited { get; set; }
    }
}
