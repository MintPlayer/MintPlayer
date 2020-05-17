using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using MintPlayer.Data.Services;

namespace MintPlayer.Web.Server.Controllers.Api.Unversioned
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagController : Controller
    {
        private readonly ITagService tagService;
        public TagController(ITagService tagService)
        {
            this.tagService = tagService;
        }

        [HttpPost("page", Name = "api-tag-page")]
        public async Task<ActionResult<Pagination.PaginationResponse<Tag>>> PageTagCategories([FromBody] Pagination.PaginationRequest<Tag> request)
        {
            var tags = await tagService.PageTags(request);
            return Ok(tags);
        }

        [HttpGet(Name = "api-tag-list")]
        public async Task<ActionResult<IEnumerable<Tag>>> Get([FromHeader]bool root_tags_only = true, [FromHeader] bool include_relations = false)
        {
            var tags = await tagService.GetTags(root_tags_only, include_relations);
            return Ok(tags);
        }

        [HttpGet("{id}", Name = "api-tag-get", Order = 1)]
        public async Task<ActionResult<Tag>> Get(int id, [FromHeader] bool include_relations = false)
        {
            var tag = await tagService.GetTag(id, include_relations);

            if (tag == null) return NotFound();
            else return Ok(tag);
        }

        [HttpPost(Name = "api-tag-create")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<Tag>> Post([FromBody] Tag tag)
        {
            var newTag = await tagService.InsertTag(tag);
            return Ok(newTag);
        }

        [HttpPut("{id}", Name = "api-tag-update")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<Tag>> Put(int id, [FromBody] Tag tag)
        {
            var updatedTag = await tagService.UpdateTag(tag);
            return Ok(updatedTag);
        }

        [HttpDelete("{id}", Name = "api-tag-delete")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult> Delete(int id)
        {
            await tagService.DeleteTag(id);
            return Ok();
        }
    }
}