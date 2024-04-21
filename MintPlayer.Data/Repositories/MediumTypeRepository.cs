using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MintPlayer.Data.Mappers;
using MintPlayer.Dtos.Dtos;

namespace MintPlayer.Data.Repositories;

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
	private readonly IHttpContextAccessor http_context;
	private readonly MintPlayerContext mintplayer_context;
	private readonly UserManager<Entities.User> user_manager;
	private readonly IMediumTypeMapper mediumTypeMapper;
	public MediumTypeRepository(
	IHttpContextAccessor http_context,
	MintPlayerContext mintplayer_context,
	UserManager<Entities.User> user_manager,
	IMediumTypeMapper mediumTypeMapper)
	{
		this.http_context = http_context;
		this.mintplayer_context = mintplayer_context;
		this.user_manager = user_manager;
		this.mediumTypeMapper = mediumTypeMapper;
	}

	public Task<IEnumerable<MediumType>> GetMediumTypes(bool include_relations, bool include_invisible_types)
	{
		var medium_types = mintplayer_context.MediumTypes
			.Where(mt => mt.Visible | include_invisible_types)
			.Select(mt => mediumTypeMapper.Entity2Dto(mt));
		return Task.FromResult<IEnumerable<MediumType>>(medium_types);
	}

	public async Task<MediumType> GetMediumType(int id, bool include_relations, bool include_invisible_types)
	{
		var medium_type = await mintplayer_context.MediumTypes
			.Where(mt => mt.Visible | include_invisible_types)
			.SingleOrDefaultAsync(mt => mt.Id == id);
		return mediumTypeMapper.Entity2Dto(medium_type);
	}

	public async Task<MediumType> InsertMediumType(MediumType mediumType)
	{
		var entity_medium_type = mediumTypeMapper.Dto2Entity(mediumType, mintplayer_context);

		var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
		entity_medium_type.UserInsert = user;

		await mintplayer_context.MediumTypes.AddAsync(entity_medium_type);
		await mintplayer_context.SaveChangesAsync();

		return mediumTypeMapper.Entity2Dto(entity_medium_type);
	}

	public async Task<MediumType> UpdateMediumType(MediumType mediumType)
	{
		var entity_medium_type = await mintplayer_context.MediumTypes
			.SingleOrDefaultAsync(mt => mt.Id == mediumType.Id);

		entity_medium_type.Description = mediumType.Description;
		//entity_medium_type.PlayerType = mediumType.PlayerType;

		var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
		entity_medium_type.UserUpdate = user;

		return mediumTypeMapper.Entity2Dto(entity_medium_type);
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
}
