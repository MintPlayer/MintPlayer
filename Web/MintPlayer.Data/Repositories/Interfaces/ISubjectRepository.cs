using System;
using System.Threading.Tasks;

namespace MintPlayer.Data.Repositories.Interfaces
{

    public interface ISubjectRepository
    {
        Task<Tuple<int, int>> GetLikes(int subjectId);
        Task<bool?> DoesLike(int subjectId);
        Task Like(int subjectId, bool like);

        Task SaveChangesAsync();
    }
}
