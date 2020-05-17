using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MintPlayer.Data.Entities
{
	internal class User : IdentityUser<Guid>
	{
		public string PictureUrl { get; set; }

		public List<Lyrics> Lyrics { get; set; }
		public List<Like> Likes { get; set; }
	}
}
