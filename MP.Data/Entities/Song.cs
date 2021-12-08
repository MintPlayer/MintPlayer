using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using MintPlayer.Data.Entities.Interfaces;

namespace MintPlayer.Data.Entities
{
	[Table("Songs")]
    internal class Song : Subject, ISoftDelete
	{
		public string Title { get; set; }
		public DateTime Released { get; set; }

		#region Text
		[NotMapped]
		public override string Text => Title;
		#endregion
        #region Description
        [NotMapped]
		public string Description
		{
			get
			{
				var hasArtists = Artists == null ? false : Artists.Any();
				if (hasArtists)
					return $"{Title} - {string.Join(" & ", Artists.Select(@as => @as.Artist.Name))}";
				else
					return Title;
			}
		}
		#endregion


		public List<ArtistSong> Artists { get; set; }
		public List<Lyrics> Lyrics { get; set; }
		public List<PlaylistSong> Tracks { get; set; }
	}
}
