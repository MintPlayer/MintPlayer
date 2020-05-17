using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MintPlayer.Data.Services
{
	public interface IMediumTypeService
	{
		Task<IEnumerable<MediumType>> GetMediumTypes(bool include_relations, bool include_invisible_types);
		Task<MediumType> GetMediumType(int id, bool include_relations, bool include_invisible_types);
		Task<MediumType> InsertMediumType(MediumType mediumType);
		Task<MediumType> UpdateMediumType(MediumType mediumType);
		Task DeleteMediumType(int medium_type_id);
	}
	internal class MediumTypeService : IMediumTypeService
	{
		private IMediumTypeRepository mediumTypeRepository;
		public MediumTypeService(IMediumTypeRepository mediumTypeRepository)
		{
			this.mediumTypeRepository = mediumTypeRepository;
		}

		public async Task<IEnumerable<MediumType>> GetMediumTypes(bool include_relations, bool include_invisible_types)
		{
			var medium_types = await mediumTypeRepository.GetMediumTypes(include_relations, include_invisible_types);
			return medium_types;
		}
		public async Task<MediumType> GetMediumType(int id, bool include_relations, bool include_invisible_types)
		{
			var medium_type = await mediumTypeRepository.GetMediumType(id, include_relations, include_invisible_types);
			return medium_type;
		}

		public async Task<MediumType> InsertMediumType(MediumType mediumType)
		{
			var medium_type = await mediumTypeRepository.InsertMediumType(mediumType);
			return medium_type;
		}

		public async Task<MediumType> UpdateMediumType(MediumType mediumType)
		{
			var medium_type = await mediumTypeRepository.UpdateMediumType(mediumType);
			await mediumTypeRepository.SaveChangesAsync();
			return medium_type;
		}

		public async Task DeleteMediumType(int medium_type_id)
		{
			await mediumTypeRepository.DeleteMediumType(medium_type_id);
			await mediumTypeRepository.SaveChangesAsync();
		}
	}
}