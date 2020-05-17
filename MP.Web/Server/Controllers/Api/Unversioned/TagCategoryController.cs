using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using MintPlayer.Data.Services;

namespace MintPlayer.Web.Server.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagCategoryController : Controller
    {
        private readonly ITagCategoryService tagCategoryService;
        public TagCategoryController(ITagCategoryService tagCategoryService)
        {
            this.tagCategoryService = tagCategoryService;
        }

        [HttpPost("page", Name = "api-tagcategory-page")]
        public async Task<ActionResult<Pagination.PaginationResponse<TagCategory>>> PageTagCategories([FromBody] Pagination.PaginationRequest<TagCategory> request)
        {
            var categories = await tagCategoryService.PageTagCategories(request);
            return Ok(categories);
        }

        [HttpGet(Name = "api-tagcategory-list")]
        public async Task<ActionResult<IEnumerable<TagCategory>>> Get([FromHeader] bool include_relations = false)
        {
            var categories = await tagCategoryService.GetTagCategories(include_relations);
            return Ok(categories);
        }

        [HttpGet("{id}", Name = "api-tagcategory-get", Order = 1)]
        public async Task<ActionResult<TagCategory>> Get(int id, [FromHeader] bool include_relations = false)
        {
            var category = await tagCategoryService.GetTagCategory(id, include_relations);

            if (category == null) return NotFound();
            else return Ok(category);
        }

        [HttpPost(Name = "api-tagcategory-create")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<TagCategory>> Post([FromBody] TagCategory tagCategory)
        {
            var newTagCategory = await tagCategoryService.InsertTagCategory(tagCategory);
            return Ok(newTagCategory);
        }

        [HttpPut("{id}", Name = "api-tagcategory-update")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<TagCategory>> Put(int id, [FromBody] TagCategory tagCategory)
        {
            var updatedTagCategory = await tagCategoryService.UpdateTagCategory(tagCategory);
            return Ok(updatedTagCategory);
        }

        [HttpDelete("{id}", Name = "api-tagcategory-delete")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult> Delete(int id)
        {
            await tagCategoryService.DeleteTagCategory(id);
            return Ok();
        }
    }
}