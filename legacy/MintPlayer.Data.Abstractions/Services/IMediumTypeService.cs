using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Dtos.Dtos;

namespace MintPlayer.Data.Abstractions.Services
{
	public interface IMediumTypeService
	{
		Task<IEnumerable<MediumType>> GetMediumTypes(bool include_relations);
		Task<MediumType> GetMediumType(int id, bool include_relations);
		Task<MediumType> InsertMediumType(MediumType mediumType);
		Task<MediumType> UpdateMediumType(MediumType mediumType);
		Task DeleteMediumType(int medium_type_id);
	}
}
