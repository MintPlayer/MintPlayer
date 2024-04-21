using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Extensions;
using MintPlayer.Data.Mappers;

namespace MintPlayer.Data.Repositories;

internal interface ITagCategoryRepository
{
	Task<Pagination.PaginationResponse<TagCategory>> PageTagCategories(Pagination.PaginationRequest<TagCategory> request);
	Task<IEnumerable<TagCategory>> GetTagCategories(bool include_relations = false);
	Task<TagCategory> GetTagCategory(int id, bool include_relations = false);
	Task<TagCategory> InsertTagCategory(TagCategory tagCategory);
	Task<TagCategory> UpdateTagCategory(TagCategory tagCategory);
	Task DeleteTagCategory(int tag_category_id);
	Task SaveChangesAsync();
}

internal class TagCategoryRepository : ITagCategoryRepository
{
	private readonly IHttpContextAccessor http_context;
	private readonly MintPlayerContext mintplayer_context;
	private readonly UserManager<Entities.User> user_manager;
	private readonly ITagCategoryMapper tagCategoryMapper;
	public TagCategoryRepository(
		IHttpContextAccessor http_context,
		MintPlayerContext mintplayer_context,
		UserManager<Entities.User> user_manager,
		ITagCategoryMapper tagCategoryMapper)
	{
		this.http_context = http_context;
		this.mintplayer_context = mintplayer_context;
		this.user_manager = user_manager;
		this.tagCategoryMapper = tagCategoryMapper;
	}

	public async Task<Pagination.PaginationResponse<TagCategory>> PageTagCategories(Pagination.PaginationRequest<TagCategory> request)
	{
		var tag_categories = mintplayer_context.TagCategories;

		// 1) Sort
		var ordered_tag_categories = request.SortDirection == System.ComponentModel.ListSortDirection.Descending
			? tag_categories.OrderByDescending(request.SortProperty)
			: tag_categories.OrderBy(request.SortProperty);

		// 2) Page
		var paged_tag_categories = ordered_tag_categories
			.Skip((request.Page - 1) * request.PerPage)
			.Take(request.PerPage);

		// 3) Convert to DTO
		var dto_tag_categories = await paged_tag_categories.Select(tag_category => tagCategoryMapper.Entity2Dto(tag_category, false)).ToListAsync();

		var count_tag_catgegories = await mintplayer_context.TagCategories.CountAsync();
		return new Pagination.PaginationResponse<TagCategory>(request, count_tag_catgegories, dto_tag_categories);
	}

	public Task<IEnumerable<TagCategory>> GetTagCategories(bool include_relations = false)
	{
		if (include_relations)
		{
			var tag_categories = mintplayer_context.TagCategories
				.Include(tc => tc.Tags)
				.Select(tc => tagCategoryMapper.Entity2Dto(tc, true));
			return Task.FromResult<IEnumerable<TagCategory>>(tag_categories);
		}
		else
		{
			var tag_categories = mintplayer_context.TagCategories
				.Select(tc => tagCategoryMapper.Entity2Dto(tc, true));
			return Task.FromResult<IEnumerable<TagCategory>>(tag_categories);
		}
	}

	public async Task<TagCategory> GetTagCategory(int id, bool include_relations = false)
	{
		if (include_relations)
		{
			var tag_category = await mintplayer_context.TagCategories
				.Include(tc => tc.Tags)
				.SingleOrDefaultAsync(tc => tc.Id == id);
			return tagCategoryMapper.Entity2Dto(tag_category, true);
		}
		else
		{
			var tag_category = await mintplayer_context.TagCategories
				.SingleOrDefaultAsync(tc => tc.Id == id);
			return tagCategoryMapper.Entity2Dto(tag_category, true);
		}
	}

	public async Task<TagCategory> InsertTagCategory(TagCategory tagCategory)
	{
		// Get current user
		var user = await user_manager.GetUserAsync(http_context.HttpContext.User);

		// Convert to entity
		var entity_tag_category = tagCategoryMapper.Dto2Entity(tagCategory, mintplayer_context);
		entity_tag_category.UserInsert = user;
		entity_tag_category.DateInsert = DateTime.Now;

		// Add to database
		await mintplayer_context.TagCategories.AddAsync(entity_tag_category);
		await mintplayer_context.SaveChangesAsync();

		var new_tag_category = tagCategoryMapper.Entity2Dto(entity_tag_category);
		return new_tag_category;
	}

	public async Task<TagCategory> UpdateTagCategory(TagCategory tagCategory)
	{
		// Find existing tag category
		var entity_tag_category = await mintplayer_context.TagCategories.FindAsync(tagCategory.Id);

		// Set new properties
		entity_tag_category.Color = tagCategory.Color;
		entity_tag_category.Description = tagCategory.Description;

		// Set UserUpdate
		var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
		entity_tag_category.UserUpdate = user;
		entity_tag_category.DateUpdate = DateTime.Now;

		// Update
		mintplayer_context.Update(entity_tag_category);
		return tagCategoryMapper.Entity2Dto(entity_tag_category);
	}

	public async Task DeleteTagCategory(int tagCategoryId)
	{
		// Find existing tag category
		var entity_tag_category = await mintplayer_context.TagCategories.FindAsync(tagCategoryId);

		// Get current user
		var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
		entity_tag_category.UserDelete = user;
		entity_tag_category.DateDelete = DateTime.Now;
	}

	public async Task SaveChangesAsync()
	{
		await mintplayer_context.SaveChangesAsync();
	}
}
