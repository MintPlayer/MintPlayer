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

namespace MintPlayer.Web.Server.Controllers.Api
{
	[ApiController]
	[Route("api/[controller]")]
	public class PersonController : Controller
	{
		private IPersonService personService;
		public PersonController(IPersonService personService)
		{
			this.personService = personService;
		}

        // POST: api/Person/page
        [EnableCors(CorsPolicies.AllowDatatables)]
        [HttpPost("page", Name = "api-person-page")]
		public async Task<ActionResult<Pagination.PaginationResponse<Person>>> PagePeople([FromBody] Pagination.PaginationRequest<Person> request)
		{
			var people = await personService.PagePeople(request);
			return Ok(people);
		}

		// GET: api/Person
		[HttpGet(Name = "api-person-list")]
		public async Task<ActionResult<IEnumerable<Person>>> Get([FromHeader]bool include_relations = false)
		{
			var people = await personService.GetPeople(include_relations, false);
			return Ok(people);
		}

		// GET: api/Person/5
		[HttpGet("{id}", Name = "api-person-get", Order = 1)]
		public async Task<ActionResult<Person>> Get(int id, [FromHeader]bool include_relations = false)
		{
			var person = await personService.GetPerson(id, include_relations, false);

			if (person == null) return null;
			else return Ok(person);
		}

		// POST: api/Person/favorite
		[HttpPost("favorite", Name = "api-person-favorite-page")]
		[Authorize]
		public async Task<ActionResult<Pagination.PaginationResponse<Person>>> PageFavoritePeople([FromBody] Pagination.PaginationRequest<Person> request)
		{
			var people = await personService.PageLikedPeople(request);
			return Ok(people);
		}

		// GET: api/Person/favorite
		[HttpGet("favorite", Name = "api-person-favorite")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<Artist>>> FavoritePeople()
		{
			var people = await personService.GetLikedPeople();
			return Ok(people);
		}

		// POST: api/Person
		[HttpPost(Name = "api-person-create")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<ActionResult<Person>> Post([FromBody] Person person)
		{
			var new_person = await personService.InsertPerson(person);
			return Ok(new_person);
		}

		// PUT: api/Person/5
		[HttpPut("{id}", Name = "api-person-update")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<ActionResult<Person>> Put(int id, [FromBody] Person person)
		{
			var updated_person = await personService.UpdatePerson(person);
			return Ok(updated_person);
		}

		// DELETE: api/ApiWithActions/5
		[HttpDelete("{id}", Name = "api-person-delete")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<ActionResult> Delete(int id)
		{
			await personService.DeletePerson(id);
			return Ok();
		}
	}
}