﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Abstractions.Services;

namespace MintPlayer.Web.Server.Controllers.Web.V3
{
	[Controller]
    [Route("web/v3/[controller]")]
    public class PersonController : Controller
    {
        private IPersonService personService;
        public PersonController(IPersonService personService)
        {
            this.personService = personService;
        }

        // POST: web/Person/page
        [HttpPost("page", Name = "web-v3-person-page")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<Pagination.PaginationResponse<Person>>> PagePeople([FromBody] Pagination.PaginationRequest<Person> request)
        {
            var people = await personService.PagePeople(request);
            return Ok(people);
        }

        // GET: web/Person
        [HttpGet(Name = "web-v3-person-list")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<IEnumerable<Person>>> Get([FromHeader] bool include_relations = false)
        {
            var people = await personService.GetPeople(include_relations);
            return Ok(people);
        }

        // GET: web/Person/5
        [HttpGet("{id}", Name = "web-v3-person-get", Order = 1)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<Person>> Get(int id, [FromHeader] bool include_relations = false)
        {
            var person = await personService.GetPerson(id, include_relations);

            if (person == null) return NotFound();
            else return Ok(person);
        }

        // POST: web/Person/favorite
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("favorite", Name = "web-v3-person-favorite-page")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<Pagination.PaginationResponse<Person>>> PageFavoriteArtists([FromBody] Pagination.PaginationRequest<Person> request)
        {
            var artists = await personService.PageLikedPeople(request);
            return Ok(artists);
        }

        // GET: web/Person/favorite
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpGet("favorite", Name = "web-v3-person-favorite")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<IEnumerable<Person>>> FavoritePeople()
        {
            var people = await personService.GetLikedPeople();
            return Ok(people);
        }

        // POST: web/Person
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost(Name = "web-v3-person-create")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<Person>> Post([FromBody] Person person)
        {
            var new_person = await personService.InsertPerson(person);
            return Ok(new_person);
        }

        // PUT: web/Person/5
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPut("{id}", Name = "web-v3-person-update")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<Person>> Put(int id, [FromBody] Person person)
        {
            try
            {
                var updated_person = await personService.UpdatePerson(person);
                return Ok(updated_person);
            }
            catch (Data.Exceptions.NotFoundException notFoundEx)
            {
                return NotFound();
            }
            catch (Data.Exceptions.ConcurrencyException<Person> concurrencyEx)
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status409Conflict, concurrencyEx.DatabaseValue);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        // DELETE: web/Person/5
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpDelete("{id}", Name = "web-v3-person-delete")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await personService.DeletePerson(id);
                return Ok();
            }
            catch (Data.Exceptions.NotFoundException notFoundEx)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
