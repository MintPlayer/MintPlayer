using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MintPlayer.Data.Entities
{
	internal abstract class Subject
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public List<Medium> Media { get; set; }
		public List<Like> Likes { get; set; }
		public List<SubjectTag> Tags { get; set; }

		[NotMapped]
		public abstract string Text { get; }

		public User UserInsert { get; set; }
		public User UserUpdate { get; set; }
		public User UserDelete { get; set; }

		public DateTime DateInsert { get; set; }
		public DateTime? DateUpdate { get; set; }
		public DateTime? DateDelete { get; set; }
	}
}
