using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MintPlayer.Web.Server.Controllers.Api
{
	[ApiController]
	[Route("api/v1/[controller]")]
	public class MediumTypeController : Controller
	{
		private IMediumTypeService mediumTypeService;
		public MediumTypeController(IMediumTypeService mediumTypeService)
		{
			this.mediumTypeService = mediumTypeService;
		}

		/// <summary>Get the medium types from the database.</summary>
		/// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
		/// <returns>A list of the medium types.</returns>
		[HttpGet(Name = "api-mediumtype-list")]
		public async Task<ActionResult<IEnumerable<MediumType>>> Get([FromHeader]bool include_relations = false)
		{
			var medium_types = await mediumTypeService.GetMediumTypes(include_relations, false);
			return Ok(medium_types);
		}

		/// <summary>Get a specific medium type from the database.</summary>
		/// <param name="id">The id of the medium type to retrieve.</param>
		/// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
		/// <returns>The medium type with the specified id.</returns>
		[HttpGet("{id}", Name = "api-mediumtype-get")]
		public async Task<ActionResult<MediumType>> Get(int id, [FromHeader]bool include_relations = false)
		{
			var medium_type = await mediumTypeService.GetMediumType(id, include_relations, false);

			if (medium_type == null) return NotFound();
			else return Ok(medium_type);
		}

		/// <summary>Creates a new medium type in the database.</summary>
		/// <param name="mediumType">Medium type to be inserted in the database.</param>
		/// <returns>The newly created medium type with the newly assigned id.</returns>
		[HttpPost(Name = "api-mediumtype-create")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult<MediumType>> Post([FromBody]MediumType mediumType)
		{
			mediumType.Visible = true;
			var medium_type = await mediumTypeService.InsertMediumType(mediumType);
			return Ok(medium_type);
		}

		/// <summary>Updates a medium type in the database.</summary>
		/// <param name="mediumType">New medium type information.</param>
		/// <returns>The updated medium type.</returns>
		[HttpPut("{id}", Name = "api-mediumtype-update")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult<MediumType>> Put([FromBody]MediumType mediumType)
		{
			var medium_type = await mediumTypeService.UpdateMediumType(mediumType);
			return Ok(medium_type);
		}

		/// <summary>Deletes a medium type from the database.</summary>
		/// <param name="id">Id of the medium type to delete.</param>
		[HttpDelete("{id}", Name = "api-mediumtype-delete")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> Delete(int id)
		{
			await mediumTypeService.DeleteMediumType(id);
			return Ok();
		}
	}
}