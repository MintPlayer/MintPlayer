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
	public class MediumTypeController : Controller
	{
		private IMediumTypeService mediumTypeService;
		public MediumTypeController(IMediumTypeService mediumTypeService)
		{
			this.mediumTypeService = mediumTypeService;
		}

		// GET: web/MediumType
		[HttpGet(Name = "web-v3-mediumtype-list")]
		public async Task<ActionResult<IEnumerable<MediumType>>> Get([FromHeader]bool include_relations = false)
		{
			var medium_types = await mediumTypeService.GetMediumTypes(include_relations, false);
			return Ok(medium_types);
		}

		// GET: web/MediumType/5
		[HttpGet("{id}", Name = "web-v3-mediumtype-get")]
		public async Task<ActionResult<MediumType>> Get(int id, [FromHeader]bool include_relations = false)
		{
			var medium_type = await mediumTypeService.GetMediumType(id, include_relations, false);

			if (medium_type == null) return NotFound();
			else return Ok(medium_type);
		}

		// POST: web/MediumType
		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPost(Name = "web-v3-mediumtype-create")]
		public async Task<ActionResult<MediumType>> Post([FromBody]MediumType mediumType)
		{
			mediumType.Visible = true;
			var medium_type = await mediumTypeService.InsertMediumType(mediumType);
			return Ok(medium_type);
		}

		// PUT: web/MediumType/5
		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPut("{id}", Name = "web-v3-mediumtype-update")]
		public async Task<ActionResult<MediumType>> Put(int id, [FromBody]MediumType mediumType)
		{
			var medium_type = await mediumTypeService.UpdateMediumType(mediumType);
			return Ok(medium_type);
		}

		// DELETE: web/MediumType/5
		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpDelete("{id}", Name = "web-v3-mediumtype-delete")]
		public async Task<ActionResult> Delete(int id)
		{
			await mediumTypeService.DeleteMediumType(id);
			return Ok();
		}
	}
}