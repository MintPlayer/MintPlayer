using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Extensions;
using MintPlayer.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Data.Mappers;

namespace MintPlayer.Data.Repositories
{
    internal interface ITagRepository
    {
        Task<PaginationResponse<Tag>> PageTags(PaginationRequest<Tag> request);
        Task<IEnumerable<Tag>> Suggest(string search_term, bool include_relations = false);
        Task<IEnumerable<Tag>> Search(string search_term);
        Task<IEnumerable<Tag>> GetTags(bool root_tags_only = true, bool include_relations = false);
        Task<Tag> GetTag(int id, bool include_relations = false);
        Task<Tag> InsertTag(Tag tag);
        Task<Tag> UpdateTag(Tag tag);
        Task DeleteTag(int tag_id);
        Task SaveChangesAsync();
    }

    internal class TagRepository : ITagRepository
    {
        private readonly IHttpContextAccessor http_context;
        private readonly MintPlayerContext mintplayer_context;
        private readonly UserManager<Entities.User> user_manager;
        private readonly TagMapper tagMapper;
        public TagRepository(
            IHttpContextAccessor http_context,
            MintPlayerContext mintplayer_context,
            UserManager<Entities.User> user_manager,
            TagMapper tagMapper)
        {
            this.http_context = http_context;
            this.mintplayer_context = mintplayer_context;
            this.user_manager = user_manager;
            this.tagMapper = tagMapper;
        }

        public async Task<PaginationResponse<Tag>> PageTags(PaginationRequest<Tag> request)
        {
            var tags = mintplayer_context.Tags
                .Include(t => t.Category);

            // 1) Sort
            var ordered_tags = request.SortDirection == System.ComponentModel.ListSortDirection.Descending
                ? tags.OrderByDescending(request.SortProperty)
                : tags.OrderBy(request.SortProperty);

            // 2) Page
            var paged_tags = ordered_tags
                .Skip((request.Page - 1) * request.PerPage)
                .Take(request.PerPage);

            // 3) Convert to DTO
            var dto_tags = await paged_tags.Select(tag => tagMapper.Entity2Dto(tag, false)).ToListAsync();

            var count_tags = await mintplayer_context.Tags.CountAsync();
            return new PaginationResponse<Tag>(request, count_tags, dto_tags);
        }

        public Task<IEnumerable<Tag>> Suggest(string search_term, bool include_relations = false)
        {
            if (include_relations)
            {
                var tag_opions = mintplayer_context.Tags
                    .Include(t => t.Category)
                    .Where(t => t.Description.Contains(search_term))
                    .Select(t => tagMapper.Entity2Dto(t, false));
                return Task.FromResult<IEnumerable<MintPlayer.Dtos.Dtos.Tag>>(tag_opions);
            }
            else
            {
                var tag_opions = mintplayer_context.Tags
                    .Where(t => t.Description.Contains(search_term))
                    .Select(t => tagMapper.Entity2Dto(t, false));
                return Task.FromResult<IEnumerable<Tag>>(tag_opions);
            }
        }

        public Task<IEnumerable<Tag>> Search(string search_term)
        {
            var tag_opions = mintplayer_context.Tags
                .Where(t => t.Description == search_term)
                .Select(t => tagMapper.Entity2Dto(t, false));
            return Task.FromResult<IEnumerable<Tag>>(tag_opions);
        }

        public Task<IEnumerable<Tag>> GetTags(bool root_tags_only = true, bool include_relations = false)
        {
            if(include_relations)
            {
                var tags = mintplayer_context.Tags
                    .Include(t => t.Category)
                    .Include(t => t.Subjects)
                        .ThenInclude(st => st.Subject)
                    .Include(t => t.Parent)
                    .Include(t => t.Children);

                var tags_filtered = root_tags_only
                    ? tags.Where(t => t.Parent == null)
                    : tags.AsQueryable();

                return Task.FromResult<IEnumerable<Tag>>(tags_filtered.Select(t => tagMapper.Entity2Dto(t, true)));
            }
            else
            {
                var tags = mintplayer_context.Tags
                    .Include(t => t.Category);

                var tags_filtered = root_tags_only
                    ? tags.Where(t => t.Parent == null)
                    : tags.AsQueryable();

                return Task.FromResult<IEnumerable<Tag>>(tags_filtered.Select(t => tagMapper.Entity2Dto(t, false)));
            }
        }

        public async Task<Tag> GetTag(int id, bool include_relations = false)
        {
            if (include_relations)
            {
                var tag = await mintplayer_context.Tags
                    .Include(t => t.Category)
                    .Include(t => t.Subjects)
                        .ThenInclude(st => st.Subject)
                    .Include(t => t.Parent)
                    .Include(t => t.Children)
                    .SingleOrDefaultAsync(t => t.Id == id);
                return tagMapper.Entity2Dto(tag, true);
            }
            else
            {
                var tag = await mintplayer_context.Tags
                    .Include(t => t.Category)
                    .SingleOrDefaultAsync(t => t.Id == id);
                return tagMapper.Entity2Dto(tag);
            }
        }

        public async Task<Tag> InsertTag(Tag tag)
        {
            // Get current user
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);

            // Convert to entity
            var entity_tag = tagMapper.Dto2Entity(tag, mintplayer_context);
            entity_tag.UserInsert = user;
            entity_tag.DateInsert = DateTime.Now;

            // Add to database
            await mintplayer_context.Tags.AddAsync(entity_tag);
            await mintplayer_context.SaveChangesAsync();

            var new_tag = tagMapper.Entity2Dto(entity_tag);
            return new_tag;
        }

        public async Task<Tag> UpdateTag(Tag tag)
        {
            // Find existing tag
            var entity_tag = await mintplayer_context.Tags.FindAsync(tag.Id);

            // Set new properties
            entity_tag.Description = tag.Description;
            entity_tag.Category = await mintplayer_context.TagCategories.FindAsync(tag.Category.Id);

            // Set UserUpdate
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            entity_tag.UserUpdate = user;
            entity_tag.DateUpdate = DateTime.Now;

            // Update
            mintplayer_context.Update(entity_tag);
            return tagMapper.Entity2Dto(entity_tag);
        }

        public async Task DeleteTag(int tag_id)
        {
            // Find existing tag
            var entity_tag = await mintplayer_context.Tags.FindAsync(tag_id);

            // Get current user
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            entity_tag.UserDelete = user;
            entity_tag.DateDelete = DateTime.Now;
        }

        public async Task SaveChangesAsync()
        {
            await mintplayer_context.SaveChangesAsync();
        }
    }
}