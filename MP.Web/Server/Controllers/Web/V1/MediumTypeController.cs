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
	public class MediumTypeController : Controller
	{
		private IMediumTypeService mediumTypeService;
		public MediumTypeController(IMediumTypeService mediumTypeService)
		{
			this.mediumTypeService = mediumTypeService;
		}

		// GET: web/MediumType
		[HttpGet(Name = "web-v1-mediumtype-list")]
		public async Task<ActionResult<IEnumerable<MediumType>>> Get([FromHeader]bool include_relations = false)
		{
			var medium_types = await mediumTypeService.GetMediumTypes(include_relations, false);
			return Ok(medium_types);
		}

		// GET: web/MediumType/5
		[HttpGet("{id}", Name = "web-v1-mediumtype-get")]
		public async Task<ActionResult<MediumType>> Get(int id, [FromHeader]bool include_relations = false)
		{
			var medium_type = await mediumTypeService.GetMediumType(id, include_relations, false);
			return Ok(medium_type);
		}

		// POST: web/MediumType
		[HttpPost(Name = "web-v1-mediumtype-create")]
		[Authorize]
		public async Task<ActionResult<MediumType>> Post([FromBody]MediumType mediumType)
		{
			mediumType.Visible = true;
			var medium_type = await mediumTypeService.InsertMediumType(mediumType);
			return Ok(medium_type);
		}

		// PUT: web/MediumType/5
		[HttpPut("{id}", Name = "web-v1-mediumtype-update")]
		[Authorize]
		public async Task<ActionResult<MediumType>> Put(int id, [FromBody]MediumType mediumType)
		{
			var medium_type = await mediumTypeService.UpdateMediumType(mediumType);
			return Ok(medium_type);
		}

		// DELETE: web/MediumType/5
		[HttpDelete("{id}", Name = "web-v1-mediumtype-delete")]
		[Authorize]
		public async Task<ActionResult> Delete(int id)
		{
			await mediumTypeService.DeleteMediumType(id);
			return Ok();
		}
	}
}