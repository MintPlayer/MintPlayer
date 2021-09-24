using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Data.Abstractions.Services;

namespace MintPlayer.Data.Services
{
    internal class TagCategoryService : ITagCategoryService
    {
        private readonly ITagCategoryRepository tagCategoryRepository;
        public TagCategoryService(ITagCategoryRepository tagCategoryRepository)
        {
            this.tagCategoryRepository = tagCategoryRepository;
        }

        public async Task<Pagination.PaginationResponse<TagCategory>> PageTagCategories(Pagination.PaginationRequest<TagCategory> request)
        {
            var categories = await tagCategoryRepository.PageTagCategories(request);
            return categories;
        }

        public async Task<IEnumerable<TagCategory>> GetTagCategories(bool include_relations = false)
        {
            var categories = await tagCategoryRepository.GetTagCategories(include_relations);
            return categories;
        }

        public async Task<TagCategory> GetTagCategory(int id, bool include_relations = false)
        {
            var category = await tagCategoryRepository.GetTagCategory(id, include_relations);
            return category;
        }

        public async Task<TagCategory> InsertTagCategory(TagCategory tagCategory)
        {
            var newTagCategory = await tagCategoryRepository.InsertTagCategory(tagCategory);
            return newTagCategory;
        }

        public async Task<TagCategory> UpdateTagCategory(TagCategory tagCategory)
        {
            var updatedTagCategory = await tagCategoryRepository.UpdateTagCategory(tagCategory);
            await tagCategoryRepository.SaveChangesAsync();
            return updatedTagCategory;
        }

        public async Task DeleteTagCategory(int tagCategoryId)
        {
            await tagCategoryRepository.DeleteTagCategory(tagCategoryId);
            await tagCategoryRepository.SaveChangesAsync();
        }
    }
}
