using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Pagination;

namespace MintPlayer.Data.Abstractions.Services
{
	public interface ITagService
	{
		Task<PaginationResponse<Tag>> PageTags(PaginationRequest<Tag> request);
		Task<IEnumerable<Tag>> Suggest(string search_term, bool include_relations = false);
		Task<IEnumerable<Tag>> Search(string search_term);
		Task<IEnumerable<Tag>> GetTags(bool root_tags_only = true, bool include_relations = false);
		Task<Tag> GetTag(int id, bool include_relations = false);
		Task<Tag> InsertTag(Tag tag);
		Task<Tag> UpdateTag(Tag tag);
		Task DeleteTag(int tag_id);
	}
}
