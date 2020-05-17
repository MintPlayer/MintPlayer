using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Services;

namespace MintPlayer.Web.Server.Controllers.Api
{
	[ApiController]
	[Route("api/[controller]")]
	public class MediumTypeController : Controller
	{
		private IMediumTypeService mediumTypeService;
		public MediumTypeController(IMediumTypeService mediumTypeService)
		{
			this.mediumTypeService = mediumTypeService;
		}

		// GET: api/MediumType
		[HttpGet(Name = "api-mediumtype-list")]
		public async Task<ActionResult<IEnumerable<MediumType>>> Get([FromHeader]bool include_relations = false)
		{
			var medium_types = await mediumTypeService.GetMediumTypes(include_relations, false);
			return Ok(medium_types);
		}

		// GET: api/MediumType/5
		[HttpGet("{id}", Name = "api-mediumtype-get")]
		public async Task<ActionResult<MediumType>> Get(int id, [FromHeader]bool include_relations = false)
		{
			var medium_type = await mediumTypeService.GetMediumType(id, include_relations, false);

			if (medium_type == null) return NotFound();
			else return Ok(medium_type);
		}

		// POST: api/MediumType
		[HttpPost(Name = "api-mediumtype-create")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<ActionResult<MediumType>> Post([FromBody]MediumType mediumType)
		{
			mediumType.Visible = true;
			var medium_type = await mediumTypeService.InsertMediumType(mediumType);
			return Ok(medium_type);
		}

		// PUT: api/MediumType/5
		[HttpPut("{id}", Name = "api-mediumtype-update")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<ActionResult<MediumType>> Put(int id, [FromBody]MediumType mediumType)
		{
			var medium_type = await mediumTypeService.UpdateMediumType(mediumType);
			return Ok(medium_type);
		}

		// DELETE: api/MediumType/5
		[HttpDelete("{id}", Name = "api-mediumtype-delete")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<ActionResult> Delete(int id)
		{
			await mediumTypeService.DeleteMediumType(id);
			return Ok();
		}
	}
}