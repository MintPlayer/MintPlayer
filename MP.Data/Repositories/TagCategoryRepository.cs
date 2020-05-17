﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MintPlayer.Data.Repositories
{
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
        public TagCategoryRepository(IHttpContextAccessor http_context, MintPlayerContext mintplayer_context, UserManager<Entities.User> user_manager)
        {
            this.http_context = http_context;
            this.mintplayer_context = mintplayer_context;
            this.user_manager = user_manager;
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
            var dto_tag_categories = paged_tag_categories.Select(tag_category => ToDto(tag_category, false));

            var count_tag_catgegories = await mintplayer_context.TagCategories.CountAsync();
            return new Pagination.PaginationResponse<TagCategory>(request, count_tag_catgegories, dto_tag_categories);
        }

        public Task<IEnumerable<TagCategory>> GetTagCategories(bool include_relations = false)
        {
            if (include_relations)
            {
                var tag_categories = mintplayer_context.TagCategories
                    .Include(tc => tc.Tags)
                    .Select(tc => ToDto(tc, true));
                return Task.FromResult<IEnumerable<TagCategory>>(tag_categories);
            }
            else
            {
                var tag_categories = mintplayer_context.TagCategories
                    .Select(tc => ToDto(tc, true));
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
                return ToDto(tag_category, true);
            }
            else
            {
                var tag_category = await mintplayer_context.TagCategories
                    .SingleOrDefaultAsync(tc => tc.Id == id);
                return ToDto(tag_category, true);
            }
        }

        public async Task<TagCategory> InsertTagCategory(TagCategory tagCategory)
        {
            // Get current user
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);

            // Convert to entity
            var entity_tag_category = ToEntity(tagCategory, mintplayer_context);
            entity_tag_category.UserInsert = user;
            entity_tag_category.DateInsert = DateTime.Now;

            // Add to database
            await mintplayer_context.TagCategories.AddAsync(entity_tag_category);
            await mintplayer_context.SaveChangesAsync();

            var new_tag_category = ToDto(entity_tag_category);
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
            return ToDto(entity_tag_category);
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

        #region Conversion methods
        internal static TagCategory ToDto(Entities.TagCategory tagCategory, bool include_relations = false)
        {
            if (tagCategory == null) return null;
            if (include_relations)
            {
                return new TagCategory
                {
                    Id = tagCategory.Id,
                    Color = tagCategory.Color,
                    Description = tagCategory.Description,
                    Tags = tagCategory.Tags == null
                        ? new List<Tag>()
                        : tagCategory.Tags.Where(t => t.Parent == null).Select(t => TagRepository.ToDto(t)).ToList(),
                    TotalTagCount = tagCategory.Tags == null
                        ? 0
                        : tagCategory.Tags.Count()
                };
            }
            else
            {
                return new TagCategory
                {
                    Id = tagCategory.Id,
                    Color = tagCategory.Color,
                    Description = tagCategory.Description
                };
            }
        }
        internal static Entities.TagCategory ToEntity(TagCategory tagCategory, MintPlayerContext mintplayer_context)
        {
            if (tagCategory == null) return null;
            var entity_tag_category = new Entities.TagCategory
            {
                Id = tagCategory.Id,
                Color = tagCategory.Color,
                Description = tagCategory.Description
            };
            return entity_tag_category;
        }
        #endregion
    }
}
