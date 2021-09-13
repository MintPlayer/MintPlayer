using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using MintPlayer.Data.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MintPlayer.Web.Server.Controllers.Api
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TagCategoryController : Controller
    {
        private readonly ITagCategoryService tagCategoryService;
        public TagCategoryController(ITagCategoryService tagCategoryService)
        {
            this.tagCategoryService = tagCategoryService;
        }

        /// <summary>Page through the tag categories in the database.</summary>
		/// <param name="request">Object containing the pagination information.</param>
        /// <returns>A slice of the tag categories.</returns>
        [HttpPost("page", Name = "api-tagcategory-page")]
        public async Task<ActionResult<Pagination.PaginationResponse<TagCategory>>> PageTagCategories([FromBody] Pagination.PaginationRequest<TagCategory> request)
        {
            var categories = await tagCategoryService.PageTagCategories(request);
            return Ok(categories);
        }

        /// <summary>Get the tag categories in the database.</summary>
        /// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
        /// <returns>A list of the tag categories.</returns>
        [HttpGet(Name = "api-tagcategory-list")]
        public async Task<ActionResult<IEnumerable<TagCategory>>> Get([FromHeader] bool include_relations = false)
        {
            var categories = await tagCategoryService.GetTagCategories(include_relations);
            return Ok(categories);
        }

        /// <summary>Get a specific tag category from the database.</summary>
        /// <param name="id">The id of the tag category to retrieve.</param>
		/// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
        /// <returns>The tag category with the specified id.</returns>
        [HttpGet("{id}", Name = "api-tagcategory-get", Order = 1)]
        public async Task<ActionResult<TagCategory>> Get(int id, [FromHeader] bool include_relations = false)
        {
            var category = await tagCategoryService.GetTagCategory(id, include_relations);

            if (category == null) return NotFound();
            else return Ok(category);
        }

        /// <summary>Creates a new tag category in the database.</summary>
        /// <param name="tagCategory">Tag category to be inserted in the database.</param>
        /// <returns>The newly created tag category with the newly assigned id.</returns>
        [HttpPost(Name = "api-tagcategory-create")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<TagCategory>> Post([FromBody] TagCategory tagCategory)
        {
            var newTagCategory = await tagCategoryService.InsertTagCategory(tagCategory);
            return Ok(newTagCategory);
        }

        /// <summary>Updates a tag category in the database.</summary>
        /// <param name="tagCategory">New tag category information.</param>
        /// <returns>The updated tag category.</returns>
        [HttpPut("{id}", Name = "api-tagcategory-update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<TagCategory>> Put([FromBody] TagCategory tagCategory)
        {
            var updatedTagCategory = await tagCategoryService.UpdateTagCategory(tagCategory);
            return Ok(updatedTagCategory);
        }

        /// <summary>Deletes a tag category from the database.</summary>
        /// <param name="id">Id of the tag category to delete.</param>
        [HttpDelete("{id}", Name = "api-tagcategory-delete")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete(int id)
        {
            await tagCategoryService.DeleteTagCategory(id);
            return Ok();
        }
    }
}