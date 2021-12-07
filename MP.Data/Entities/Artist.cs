using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MintPlayer.Data.Entities.Interfaces;

namespace MintPlayer.Data.Entities
{
	[Table("Artists", Schema = "mintplay")]
	internal class Artist : Subject, ISoftDelete
	{
		public string Name { get; set; }
		public int? YearStarted { get; set; }
		public int? YearQuit { get; set; }

		[NotMapped]
		public override string Text => Name;

		public List<ArtistPerson> Members { get; set; }
		public List<ArtistSong> Songs { get; set; }
	}
}
