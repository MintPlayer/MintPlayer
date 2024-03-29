﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Abstractions.Services;

namespace MintPlayer.Web.Server.Controllers.Web.V3
{
	[Controller]
    [Route("web/v3/[controller]")]
    public class TagCategoryController : Controller
    {
        private readonly ITagCategoryService tagCategoryService;
        public TagCategoryController(ITagCategoryService tagCategoryService)
        {
            this.tagCategoryService = tagCategoryService;
        }

        [HttpPost("page", Name = "web-v3-tagcategory-page")]
		[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<Pagination.PaginationResponse<TagCategory>>> PageTagCategories([FromBody] Pagination.PaginationRequest<TagCategory> request)
        {
            var categories = await tagCategoryService.PageTagCategories(request);
            return Ok(categories);
        }

        [HttpGet(Name = "web-v3-tagcategory-list")]
		[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<IEnumerable<TagCategory>>> Get([FromHeader] bool include_relations = false)
        {
            var categories = await tagCategoryService.GetTagCategories(include_relations);
            return Ok(categories);
        }

        [HttpGet("{id}", Name = "web-v3-tagcategory-get", Order = 1)]
		[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<TagCategory>> Get(int id, [FromHeader] bool include_relations = false)
        {
            var category = await tagCategoryService.GetTagCategory(id, include_relations);

            if (category == null) return NotFound();
            else return Ok(category);
        }

        [Authorize]
		[ValidateAntiForgeryToken]
        [HttpPost(Name = "web-v3-tagcategory-create")]
		[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<TagCategory>> Post([FromBody] TagCategory tagCategory)
        {
            var newTagCategory = await tagCategoryService.InsertTagCategory(tagCategory);
            return Ok(newTagCategory);
        }

        [Authorize]
		[ValidateAntiForgeryToken]
        [HttpPut("{id}", Name = "web-v3-tagcategory-update")]
		[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<TagCategory>> Put(int id, [FromBody] TagCategory tagCategory)
        {
            var updatedTagCategory = await tagCategoryService.UpdateTagCategory(tagCategory);
            return Ok(updatedTagCategory);
        }

        [Authorize]
		[ValidateAntiForgeryToken]
        [HttpDelete("{id}", Name = "web-v3-tagcategory-delete")]
		[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> Delete(int id)
        {
            await tagCategoryService.DeleteTagCategory(id);
            return Ok();
        }
    }
}
