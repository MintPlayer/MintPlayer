using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using MintPlayer.Data.Abstractions.Services;

namespace MintPlayer.Data.Services;

internal class MediumTypeService : IMediumTypeService
{
	private readonly IMediumTypeRepository mediumTypeRepository;
	private readonly UserManager<Entities.User> userManager;
	private readonly IHttpContextAccessor httpContextAccessor;
	public MediumTypeService(IMediumTypeRepository mediumTypeRepository, UserManager<Entities.User> userManager, IHttpContextAccessor httpContextAccessor)
	{
		this.mediumTypeRepository = mediumTypeRepository;
		this.userManager = userManager;
		this.httpContextAccessor = httpContextAccessor;
	}

	public async Task<IEnumerable<MediumType>> GetMediumTypes(bool include_relations)
	{
		var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
		var isAdmin = user == null ? false : await userManager.IsInRoleAsync(user, "Administrator");
		var medium_types = await mediumTypeRepository.GetMediumTypes(include_relations, isAdmin);
		return medium_types;
	}
	public async Task<MediumType> GetMediumType(int id, bool include_relations)
	{
		var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
		var isAdmin = user == null ? false : await userManager.IsInRoleAsync(user, "Administrator");
		var medium_type = await mediumTypeRepository.GetMediumType(id, include_relations, isAdmin);
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
