using MintPlayer.Dtos.Dtos;

namespace MintPlayer.Data.Abstractions.Services;

public interface ITagCategoryService
{
	Task<Pagination.PaginationResponse<TagCategory>> PageTagCategories(Pagination.PaginationRequest<TagCategory> request);
	Task<IEnumerable<TagCategory>> GetTagCategories(bool include_relations = false);
	Task<TagCategory> GetTagCategory(int id, bool include_relations = false);
	Task<TagCategory> InsertTagCategory(TagCategory tagCategory);
	Task<TagCategory> UpdateTagCategory(TagCategory tagCategory);
	Task DeleteTagCategory(int tag_category_id);
}
