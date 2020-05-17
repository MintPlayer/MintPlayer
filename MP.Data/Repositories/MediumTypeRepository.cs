using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MintPlayer.Dtos.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MintPlayer.Data.Repositories
{
	internal interface IMediumTypeRepository
	{
		Task<IEnumerable<MediumType>> GetMediumTypes(bool include_relations, bool include_invisible_types);
		Task<MediumType> GetMediumType(int id, bool include_relations, bool include_invisible_types);
		Task<MediumType> InsertMediumType(MediumType mediumType);
		Task<MediumType> UpdateMediumType(MediumType mediumType);
		Task DeleteMediumType(int medium_type_id);
		Task SaveChangesAsync();
	}
	internal class MediumTypeRepository : IMediumTypeRepository
	{
		private IHttpContextAccessor http_context;
		private MintPlayerContext mintplayer_context;
		private UserManager<Entities.User> user_manager;
		public MediumTypeRepository(IHttpContextAccessor http_context, MintPlayerContext mintplayer_context, UserManager<Entities.User> user_manager)
		{
			this.http_context = http_context;
			this.mintplayer_context = mintplayer_context;
			this.user_manager = user_manager;
		}

		public Task<IEnumerable<MediumType>> GetMediumTypes(bool include_relations, bool include_invisible_types)
		{
			var medium_types = mintplayer_context.MediumTypes
				.Where(mt => mt.Visible | include_invisible_types)
				.Select(mt => ToDto(mt));
			return Task.FromResult<IEnumerable<MediumType>>(medium_types);
		}

		public async Task<MediumType> GetMediumType(int id, bool include_relations, bool include_invisible_types)
		{
			var medium_type = await mintplayer_context.MediumTypes
				.Where(mt => mt.Visible | include_invisible_types)
				.SingleOrDefaultAsync(mt => mt.Id == id);
			return ToDto(medium_type);
		}

		public async Task<MediumType> InsertMediumType(MediumType mediumType)
		{
			var entity_medium_type = ToEntity(mediumType, mintplayer_context);

			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			entity_medium_type.UserInsert = user;

			await mintplayer_context.MediumTypes.AddAsync(entity_medium_type);
			await mintplayer_context.SaveChangesAsync();

			return ToDto(entity_medium_type);
		}

		public async Task<MediumType> UpdateMediumType(MediumType mediumType)
		{
			var entity_medium_type = await mintplayer_context.MediumTypes
				.SingleOrDefaultAsync(mt => mt.Id == mediumType.Id);

			entity_medium_type.Description = mediumType.Description;
			entity_medium_type.PlayerType = mediumType.PlayerType;

			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			entity_medium_type.UserUpdate = user;

			return ToDto(entity_medium_type);
		}

		public async Task DeleteMediumType(int medium_type_id)
		{
			var medium_type = mintplayer_context.MediumTypes.Find(medium_type_id);
			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			medium_type.UserDelete = user;
		}

		public async Task SaveChangesAsync()
		{
			await mintplayer_context.SaveChangesAsync();
		}

		#region Conversion Methods
		internal static MediumType ToDto(Entities.MediumType mediumType)
		{
			if (mediumType == null) return null;
			return new MediumType
			{
				Id = mediumType.Id,
				Description = mediumType.Description,
				PlayerType = mediumType.PlayerType,
				Visible = mediumType.Visible
			};
		}
		internal static Entities.MediumType ToEntity(MediumType mediumType, MintPlayerContext mintplayer_context)
		{
			if (mediumType == null) return null;
			return new Entities.MediumType
			{
				Id = mediumType.Id,
				Description = mediumType.Description,
				PlayerType = mediumType.PlayerType,
				Visible = mediumType.Visible
			};
		}
		#endregion
	}
}
