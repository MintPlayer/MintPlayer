using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MintPlayer.Data.Abstractions.Services;

namespace MintPlayer.Web.Server.Controllers.Api
{
	[ApiController]
    [Route("api/v1/[controller]")]
    public class PersonController : Controller
    {
        private IPersonService personService;
        public PersonController(IPersonService personService)
        {
            this.personService = personService;
        }

        /// <summary>Page through the people in the database.</summary>
        /// <param name="request">Object containing the pagination information.</param>
        /// <returns>A slice of the people.</returns>
        [EnableCors(CorsPolicies.AllowDatatables)]
        [HttpPost("page", Name = "api-person-page")]
        public async Task<ActionResult<Pagination.PaginationResponse<Person>>> PagePeople([FromBody] Pagination.PaginationRequest<Person> request)
        {
            var people = await personService.PagePeople(request);
            return Ok(people);
        }

        /// <summary>Get the people in the database.</summary>
        /// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
        /// <returns>A list of the people.</returns>
        [HttpGet(Name = "api-person-list")]
        public async Task<ActionResult<IEnumerable<Person>>> Get([FromHeader] bool include_relations = false)
        {
            var people = await personService.GetPeople(include_relations);
            return Ok(people);
        }

        /// <summary>Get a specific person from the database.</summary>
        /// <param name="id">The id of the person to retrieve.</param>
        /// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
        /// <returns>The person with the specified id.</returns>
        [HttpGet("{id}", Name = "api-person-get", Order = 1)]
        public async Task<ActionResult<Person>> Get(int id, [FromHeader] bool include_relations = false)
        {
            var person = await personService.GetPerson(id, include_relations);

            if (person == null) return null;
            else return Ok(person);
        }

        /// <summary>Get the favorite people for the current user.</summary>
        /// <param name="request">Object containing the pagination information.</param>
        /// <returns>A slice of the favorite people for the current user.</returns>
        [HttpPost("favorite", Name = "api-person-favorite-page")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Pagination.PaginationResponse<Person>>> PageFavoritePeople([FromBody] Pagination.PaginationRequest<Person> request)
        {
            var people = await personService.PageLikedPeople(request);
            return Ok(people);
        }

        /// <summary>Get the favorite people for the current user.</summary>
        /// <returns>The favorite people for the current user.</returns>
        [HttpGet("favorite", Name = "api-person-favorite")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<Artist>>> FavoritePeople()
        {
            var people = await personService.GetLikedPeople();
            return Ok(people);
        }

        /// <summary>Creates a new person in the database.</summary>
        /// <param name="artist">Person to be inserted in the database.</param>
        /// <returns>The newly created person with the newly assigned id.</returns>
        [HttpPost(Name = "api-person-create")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Person>> Post([FromBody] Person person)
        {
            var new_person = await personService.InsertPerson(person);
            return Ok(new_person);
        }

        /// <summary>Updates a person in the database.</summary>
        /// <param name="person">New person information</param>
        /// <returns>The updated person.</returns>
        [HttpPut("{id}", Name = "api-person-update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Person>> Put([FromBody] Person person)
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

        /// <summary>Deletes a person from the database.</summary>
        /// <param name="id">Id of the person to delete.</param>
        [HttpDelete("{id}", Name = "api-person-delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
