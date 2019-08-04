using System;
using System.Collections.Generic;
using System.Text;

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
            UserId = user?.Id ?? 0;
            DoesLike = like;
        }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public bool DoesLike { get; set; }
    }
}
