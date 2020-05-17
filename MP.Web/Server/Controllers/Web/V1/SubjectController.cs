using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using MintPlayer.Data.Services;

namespace MintPlayer.Web.Server.Controllers.Web.V1
{
	[Controller]
	[Route("web/v1/[controller]")]
	public class SubjectController : Controller
	{
		private ISubjectService subjectService;
		public SubjectController(ISubjectService subjectService)
		{
			this.subjectService = subjectService;
		}

		[HttpGet("{subject_id}/likes", Name = "web-v1-subject-getlikes")]
		public async Task<ActionResult<SubjectLikeResult>> Likes([FromRoute]int subject_id)
		{
			var result = await subjectService.GetLikes(subject_id);
			return Ok(result);
		}

		[Authorize]
		[HttpPost("{subject_id}/likes", Name = "web-v1-subject-like")]
		public async Task<ActionResult<SubjectLikeResult>> Like([FromRoute]int subject_id, [FromBody]bool like)
		{
			var result = await subjectService.Like(subject_id, like);
			return Ok(result);
		}

		[HttpGet("search/suggest/{subjects_concat}/{search_term}", Name = "web-v1-subject-suggest")]
		public async Task<ActionResult<IEnumerable<Subject>>> Suggest([FromRoute]string subjects_concat, [FromRoute]string search_term, [FromHeader]bool include_relations = false)
		{
			var subjects = subjects_concat.Split('-');
			var suggestions = await subjectService.Suggest(subjects, search_term, include_relations, false);
			return Ok(suggestions);
		}

		[HttpGet("search/{subjects_concat}/{search_term}", Name = "web-v1-subject-search")]
		public async Task<ActionResult<SearchResults>> Search([FromRoute]string subjects_concat, [FromRoute]string search_term)
		{
			var subjects = subjects_concat.Split('-');
			var results = await subjectService.Search(subjects, search_term, false, false, false);
			return Ok(results);
		}
	}
}