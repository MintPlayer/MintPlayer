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
		/// <param name="subjects_concat">A dash-seperated string containing the subject-types (person, artist, song).</param>
		/// <param name="search_term">Search term to get suggestions for.</param>
		/// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
		/// <returns>A list of subjects where the name starts with the specified search term.</returns>
		[EnableCors(CorsPolicies.AllowDatatables)]
		[HttpGet("search/suggest/{subjects_concat}/{search_term}", Name = "api-subject-suggest")]
		public async Task<ActionResult<IEnumerable<Subject>>> Suggest([FromRoute]string subjects_concat, [FromRoute]string search_term, [FromHeader]bool include_relations = false)
		{
			var subjects = subjects_concat.Split('-');
			var suggestions = await subjectService.Suggest(subjects, search_term, include_relations, false);
			return Ok(suggestions.ToArray());
		}

		/// <summary>Search for subjects using a search term.</summary>
		/// <param name="subjects_concat">A dash-seperated string containing the subject-types (person, artist, song).</param>
		/// <param name="search_term">Search term to search for.</param>
		/// <returns>A list of subjects matching the search term.</returns>
		[HttpGet("search/{subjects_concat}/{search_term}", Name = "api-subject-search")]
		public async Task<ActionResult<SearchResults>> Search([FromRoute]string subjects_concat, [FromRoute]string search_term)
		{
			var subjects = subjects_concat.Split('-');
			var results = await subjectService.Search(subjects, search_term, false, false, false);
			return Ok(results);
		}
	}
}