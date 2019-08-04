using MintPlayer.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MintPlayer.Data.Repositories.Interfaces
{

    public interface ISubjectRepository
    {
        Task<List<Subject>> Suggest(string[] subjects, string search_term);
        Task<List<Subject>> Search(string[] subjects, string search_term);

        Task<Tuple<int, int>> GetLikes(int subjectId);
        Task<bool?> DoesLike(int subjectId);
        Task Like(int subjectId, bool like);

        Task SaveChangesAsync();
    }
}
