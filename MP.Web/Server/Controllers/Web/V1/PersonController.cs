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
	public class PersonController : Controller
	{
		private IPersonService personService;
		public PersonController(IPersonService personService)
		{
			this.personService = personService;
		}

		// POST: web/Person/page
		[HttpPost("page", Name = "web-v1-person-page")]
		public async Task<ActionResult<Pagination.PaginationResponse<Person>>> PagePeople([FromBody] Pagination.PaginationRequest<Person> request)
		{
			var people = await personService.PagePeople(request);
			return Ok(people);
		}

		// GET: web/Person
		[HttpGet(Name = "web-v1-person-list")]
		public async Task<ActionResult<IEnumerable<Person>>> Get([FromHeader]bool include_relations = false)
		{
			var people = await personService.GetPeople(include_relations, false);
			return Ok(people);
		}

		// GET: web/Person/5
		[HttpGet("{id}", Name = "web-v1-person-get", Order = 1)]
		public async Task<ActionResult<Person>> Get(int id, [FromHeader]bool include_relations = false)
		{
			var person = await personService.GetPerson(id, include_relations, false);
			return Ok(person);
		}

		// POST: web/Person
		[HttpPost(Name = "web-v1-person-create")]
		[Authorize]
		public async Task<ActionResult<Person>> Post([FromBody] Person person)
		{
			var new_person = await personService.InsertPerson(person);
			return Ok(new_person);
		}

		// PUT: web/Person/5
		[HttpPut("{id}", Name = "web-v1-person-update")]
		[Authorize]
		public async Task<ActionResult<Person>> Put(int id, [FromBody] Person person)
		{
			var updated_person = await personService.UpdatePerson(person);
			return Ok(updated_person);
		}

		// DELETE: web/Person/5
		[HttpDelete("{id}", Name = "web-v1-person-delete")]
		[Authorize]
		public async Task<ActionResult> Delete(int id)
		{
			await personService.DeletePerson(id);
			await personService.DeletePerson(id);
			return Ok();
		}
	}
}