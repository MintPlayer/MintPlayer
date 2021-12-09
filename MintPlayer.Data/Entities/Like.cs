using System;

namespace MintPlayer.Data.Entities
{
	internal class Like
	{
		public Like()
		{
		}

		public Like(Subject subject, User user, bool like) : this()
		{
			Subject = subject;
			SubjectId = subject?.Id ?? 0;
			User = user;
			UserId = user == null ? Guid.Empty : user.Id;
			DoesLike = like;
		}

		public int SubjectId { get; set; }
		public Subject Subject { get; set; }

		public Guid UserId { get; set; }
		public User User { get; set; }

		public bool DoesLike { get; set; }
	}
}
