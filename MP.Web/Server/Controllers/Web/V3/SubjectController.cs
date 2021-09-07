using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using MintPlayer.Data.Services;

namespace MintPlayer.Web.Server.Controllers.Web.V3
{
	[Controller]
	[Route("web/v3/[controller]")]
	public class SubjectController : Controller
	{
		private ISubjectService subjectService;
		public SubjectController(ISubjectService subjectService)
		{
			this.subjectService = subjectService;
		}

		[HttpGet("{subject_id}/likes", Name = "web-v3-subject-getlikes")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<SubjectLikeResult>> Likes([FromRoute]int subject_id)
		{
			var result = await subjectService.GetLikes(subject_id);
			return Ok(result);
		}

		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPost("{subject_id}/likes", Name = "web-v3-subject-like")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<SubjectLikeResult>> Like([FromRoute]int subject_id, [FromBody]bool like)
		{
			var result = await subjectService.Like(subject_id, like);
			return Ok(result);
		}

		[Authorize]
		[HttpGet("favorite", Name = "web-v3-subject-favorite")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<IEnumerable<Subject>>> FavoriteSubjects()
		{
			var result = await subjectService.GetLikedSubjects();
			return Ok(result);
		}

		[HttpPost("search/suggest", Name = "web-v3-subject-suggest")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<IEnumerable<Subject>>> Suggest([FromBody] SearchRequest searchRequest, [FromHeader]bool include_relations = false)
		{
			var suggestions = await subjectService.Suggest(searchRequest.SubjectTypes, searchRequest.SearchTerm, include_relations, false);
			return Ok(suggestions.ToArray());
		}

		[HttpPost("search", Name = "web-v3-subject-search")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<SearchResults>> Search([FromBody] SearchRequest searchRequest)
		{
			var results = await subjectService.Search(searchRequest.SubjectTypes, searchRequest.SearchTerm, false, false, false);
			return Ok(results);
		}
	}
}
