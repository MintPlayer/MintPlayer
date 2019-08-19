using System.Threading.Tasks;
using System.Collections.Generic;

namespace MintPlayer.Data.Repositories.Interfaces
{

    public interface IPersonRepository
    {
        IEnumerable<Dtos.Person> GetPeople(bool include_relations = false);
        IEnumerable<Dtos.Person> GetPeople(int count, int page, bool include_relations = false);
        Dtos.Person GetPerson(int id, bool include_relations = false);
        Task<Dtos.Person> InsertPerson(Dtos.Person person);
        Task<Dtos.Person> UpdatePerson(Dtos.Person person);
        Task DeletePerson(int person_id);
        Task SaveChangesAsync();
    }
}
