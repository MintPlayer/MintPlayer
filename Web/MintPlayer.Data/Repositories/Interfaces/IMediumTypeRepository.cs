using System.Threading.Tasks;
using System.Collections.Generic;

namespace MintPlayer.Data.Repositories.Interfaces
{
    public interface IMediumTypeRepository
    {
        IEnumerable<Dtos.MediumType> GetMediumTypes(bool include_relations = false);
        Dtos.MediumType GetMediumType(int id, bool include_relations = false);
        Task<Dtos.MediumType> InsertMediumType(Dtos.MediumType mediumType);
        Task<Dtos.MediumType> UpdateMediumType(Dtos.MediumType mediumType);
        Task DeleteMediumType(int medium_type_id);
        Task SaveChangesAsync();
    }
}
