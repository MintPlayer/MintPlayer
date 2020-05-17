using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using MintPlayer.Data.Services;
using MintPlayer.Web.Server.ViewModels.Tag;

namespace MintPlayer.Web.Server.Controllers.Web.V2
{
    [Controller]
    [Route("web/v2/[controller]")]
    public class TagController : Controller
    {
        private readonly ITagService tagService;
        public TagController(ITagService tagService)
        {
            this.tagService = tagService;
        }

        [HttpPost("page", Name = "web-v2-tag-page")]
        public async Task<ActionResult<Pagination.PaginationResponse<Tag>>> PageTagCategories([FromBody] Pagination.PaginationRequest<Tag> request)
        {
            var tags = await tagService.PageTags(request);
            return Ok(tags);
        }

        [HttpPost("suggest", Name = "web-v2-tag-suggest")]
        public async Task<ActionResult<IEnumerable<Tag>>> Suggest([FromBody] SuggestVM suggestVM, [FromHeader(Name = "include_relations")] bool include_relations = false)
        {
            var tags = await tagService.Suggest(suggestVM.SearchTerm, include_relations);
            return Ok(tags);
        }

        [HttpPost("search", Name = "web-v2-tag-search")]
        public async Task<ActionResult<IEnumerable<Tag>>> Search([FromBody] SearchVM searchVM)
        {
            var tags = await tagService.Search(searchVM.SearchTerm);
            return Ok(tags);
        }

        [HttpGet(Name = "web-v2-tag-list")]
        public async Task<ActionResult<IEnumerable<Tag>>> Get([FromHeader]bool root_tags_only = true, [FromHeader] bool include_relations = false)
        {
            var tags = await tagService.GetTags(root_tags_only, include_relations);
            return Ok(tags);
        }

        [HttpGet("{id}", Name = "web-v2-tag-get", Order = 1)]
        public async Task<ActionResult<Tag>> Get(int id, [FromHeader] bool include_relations = false)
        {
            var tag = await tagService.GetTag(id, include_relations);

            if (tag == null) return NotFound();
            else return Ok(tag);
        }

        [HttpPost(Name = "web-v2-tag-create")]
        [Authorize]
        public async Task<ActionResult<Tag>> Post([FromBody] Tag tag)
        {
            var newTag = await tagService.InsertTag(tag);
            return Ok(newTag);
        }

        [HttpPut("{id}", Name = "web-v2-tag-update")]
        [Authorize]
        public async Task<ActionResult<Tag>> Put(int id, [FromBody] Tag tag)
        {
            var updatedTag = await tagService.UpdateTag(tag);
            return Ok(updatedTag);
        }

        [HttpDelete("{id}", Name = "web-v2-tag-delete")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            await tagService.DeleteTag(id);
            return Ok();
        }
    }
}