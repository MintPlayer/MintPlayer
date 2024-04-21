using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MintPlayer.Data.Abstractions.Services;

namespace MintPlayer.Web.Server.Controllers.Api.Unversioned;

[ApiController]
[Route("api/v1/[controller]")]
public class TagController : Controller
{
	private readonly ITagService tagService;
	public TagController(ITagService tagService)
	{
		this.tagService = tagService;
	}

	/// <summary>Page through the tags in the database.</summary>
	/// <param name="request">Object containing the pagination information.</param>
	/// <returns>A slice of the tags.</returns>
	[HttpPost("page", Name = "api-tag-page")]
	public async Task<ActionResult<Pagination.PaginationResponse<Tag>>> PageTagCategories([FromBody] Pagination.PaginationRequest<Tag> request)
	{
		var tags = await tagService.PageTags(request);
		return Ok(tags);
	}

	/// <summary>Get the tags in the database.</summary>
	/// <param name="root_tags_only">Specifies if only the root tags should be returned.</param>
	/// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
	/// <returns>A list of the tags.</returns>
	[HttpGet(Name = "api-tag-list")]
	public async Task<ActionResult<IEnumerable<Tag>>> Get([FromHeader] bool root_tags_only = true, [FromHeader] bool include_relations = false)
	{
		var tags = await tagService.GetTags(root_tags_only, include_relations);
		return Ok(tags);
	}

	/// <summary>Get a specific tag from the database.</summary>
	/// <param name="id">The id of the tag to retrieve.</param>
	/// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
	/// <returns>The tag with the specified id.</returns>
	[HttpGet("{id}", Name = "api-tag-get", Order = 1)]
	public async Task<ActionResult<Tag>> Get(int id, [FromHeader] bool include_relations = false)
	{
		var tag = await tagService.GetTag(id, include_relations);

		if (tag == null) return NotFound();
		else return Ok(tag);
	}

	/// <summary>Creates a new tag in the database.</summary>
	/// <param name="tag">Tag to be inserted in the database.</param>
	/// <returns>The newly created tag with the newly assigned id.</returns>
	[HttpPost(Name = "api-tag-create")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<Tag>> Post([FromBody] Tag tag)
	{
		var newTag = await tagService.InsertTag(tag);
		return Ok(newTag);
	}

	/// <summary>Updates a tag in the database.</summary>
	/// <param name="tag">New tag information.</param>
	/// <returns>The updated tag.</returns>
	[HttpPut("{id}", Name = "api-tag-update")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<Tag>> Put([FromBody] Tag tag)
	{
		var updatedTag = await tagService.UpdateTag(tag);
		return Ok(updatedTag);
	}

	/// <summary>Deletes a tag from the database.</summary>
	/// <param name="id">Id of the tag to delete.</param>
	[HttpDelete("{id}", Name = "api-tag-delete")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult> Delete(int id)
	{
		await tagService.DeleteTag(id);
		return Ok();
	}
}
