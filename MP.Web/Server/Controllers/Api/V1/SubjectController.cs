using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using MintPlayer.Data.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MintPlayer.Web.Server.Controllers.Api
{
	[ApiController]
	[Route("api/v1/[controller]")]
	public class SubjectController : Controller
	{
		private ISubjectService subjectService;
		public SubjectController(ISubjectService subjectService)
		{
			this.subjectService = subjectService;
		}

		/// <summary>Get the number of likes/dislikes for a subject.</summary>
		/// <param name="subject_id">Id of the person/artist/song.</param>
		/// <returns>An object containing the like information.</returns>
		[HttpGet("{subject_id}/likes", Name = "api-subject-getlikes")]
		public async Task<ActionResult<SubjectLikeResult>> Likes([FromRoute]int subject_id)
		{
			var result = await subjectService.GetLikes(subject_id);
			return Ok(result);
		}

		/// <summary>Like/dislike a specific subject.</summary>
		/// <param name="subject_id">Id of the person/artist/song.</param>
		/// <param name="like">Like = true, dislike = false.</param>
		/// <returns>An object containing the like information.</returns>
		[HttpPost("{subject_id}/likes", Name = "api-subject-like")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult<SubjectLikeResult>> Like([FromRoute]int subject_id, [FromBody]bool like)
		{
			var result = await subjectService.Like(subject_id, like);
			return Ok(result);
		}

		/// <summary>Get the favorite subjects for the current user.</summary>
		/// <returns>A list containing the liked subjects for the current user.</returns>
		[HttpGet("favorite", Name = "api-subject-favorite")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult<IEnumerable<Subject>>> FavoriteSubjects()
		{
			var result = await subjectService.GetLikedSubjects();
			return Ok(result);
		}

		/// <summary>Get suggestions for a search term.</summary>
		/// <param name="searchRequest">Object containing subject types to search for, and the search term.</param>
		/// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
		/// <returns>A list of subjects where the name starts with the specified search term.</returns>
		[EnableCors(CorsPolicies.AllowDatatables)]
		[HttpPost("search/suggest", Name = "api-subject-suggest")]
		public async Task<ActionResult<IEnumerable<Subject>>> Suggest([FromBody] SearchRequest searchRequest, [FromHeader]bool include_relations = false)
		{
			var suggestions = await subjectService.Suggest(searchRequest.SubjectTypes, searchRequest.SearchTerm, include_relations, false);
			return Ok(suggestions.ToArray());
		}

		/// <summary>Search for subjects using a search term.</summary>
		/// <param name="searchRequest">Object containing subject types to search for, and the search term.</param>
		/// <returns>A list of subjects matching the search term.</returns>
		[HttpPost("search", Name = "api-subject-search")]
		public async Task<ActionResult<SearchResults>> Search([FromBody] SearchRequest searchRequest)
		{
			var results = await subjectService.Search(searchRequest.SubjectTypes, searchRequest.SearchTerm, false, false, false);
			return Ok(results);
		}
	}
}
