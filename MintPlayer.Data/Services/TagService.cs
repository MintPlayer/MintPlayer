using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using MintPlayer.Pagination;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Data.Abstractions.Services;

namespace MintPlayer.Data.Services
{
    internal class TagService : ITagService
    {
        private readonly ITagRepository tagRepository;
        public TagService(ITagRepository tagRepository)
        {
            this.tagRepository = tagRepository;
        }

        public async Task<PaginationResponse<Tag>> PageTags(PaginationRequest<Tag> request)
        {
            var tags = await tagRepository.PageTags(request);
            return tags;
        }

        public async Task<IEnumerable<Tag>> Suggest(string search_term, bool include_relations = false)
        {
            var tags = await tagRepository.Suggest(search_term, include_relations);
            return tags;
        }

        public async Task<IEnumerable<Tag>> Search(string search_term)
        {
            var tags = await tagRepository.Search(search_term);
            return tags;
        }

        public async Task<IEnumerable<Tag>> GetTags(bool root_tags_only = true, bool include_relations = false)
        {
            var tags = await tagRepository.GetTags(root_tags_only, include_relations);
            return tags;
        }

        public async Task<Tag> GetTag(int id, bool include_relations = false)
        {
            var tag = await tagRepository.GetTag(id, include_relations);
            return tag;
        }

        public async Task<Tag> InsertTag(Tag tag)
        {
            var newTag = await tagRepository.InsertTag(tag);
            return newTag;
        }

        public async Task<Tag> UpdateTag(Tag tag)
        {
            var updatedTag = await tagRepository.UpdateTag(tag);
            await tagRepository.SaveChangesAsync();
            return updatedTag;
        }

        public async Task DeleteTag(int tag_id)
        {
            await tagRepository.DeleteTag(tag_id);
            await tagRepository.SaveChangesAsync();
        }
    }
}
