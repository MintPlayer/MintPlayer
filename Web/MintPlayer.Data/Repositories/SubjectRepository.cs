using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MintPlayer.Data.Repositories.Interfaces;

namespace MintPlayer.Data.Repositories
{
    internal class SubjectRepository : ISubjectRepository
    {
        private IHttpContextAccessor http_context;
        private MintPlayerContext mintplayer_context;
        private UserManager<Entities.User> user_manager;
        public SubjectRepository(IHttpContextAccessor http_context, MintPlayerContext mintplayer_context, UserManager<Entities.User> user_manager)
        {
            this.http_context = http_context;
            this.mintplayer_context = mintplayer_context;
            this.user_manager = user_manager;
        }

        public async Task<Tuple<int, int>> GetLikes(int subjectId)
        {
            var likes = mintplayer_context.Likes
                .Where(l => l.SubjectId == subjectId)
                .Where(l => l.DoesLike)
                .Count();
            var dislikes = mintplayer_context.Likes
                .Where(l => l.SubjectId == subjectId)
                .Where(l => !l.DoesLike)
                .Count();
            return new Tuple<int, int>(likes, dislikes);
        }

        public async Task<bool?> DoesLike(int subjectId)
        {
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            var user_id = user?.Id ?? 0;

            if (user_id == 0) throw new UnauthorizedAccessException();

            var like = mintplayer_context.Likes
                .Where(l => l.SubjectId == subjectId)
                .Where(l => l.UserId == user_id)
                .FirstOrDefault();

            if (like == null) return null;
            else return like.DoesLike;
        }

        public async Task Like(int subjectId, bool like)
        {
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            var subject_db = await mintplayer_context.FindAsync<Entities.Subject>(subjectId);

            var existing_like = mintplayer_context.Likes
                .Where(l => l.SubjectId == subjectId)
                .Where(l => l.UserId == user.Id)
                .FirstOrDefault();

            if (existing_like == null)
            {
                var new_like = new Entities.Like(subject_db, user, like);
                await mintplayer_context.AddAsync(new_like);
            }
            else
            {
                existing_like.DoesLike = like;
                mintplayer_context.Update(existing_like);
            }
        }

        public async Task SaveChangesAsync()
        {
            await mintplayer_context.SaveChangesAsync();
        }
    }
}
