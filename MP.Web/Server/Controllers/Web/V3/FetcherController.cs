using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Data.Services;
using MintPlayer.Fetcher.Abstractions;
using MintPlayer.Web.Server.ViewModels.Fetcher;

namespace MintPlayer.Web.Server.Controllers.Web.V3
{
	[Route("web/v3/[controller]")]
	public class FetcherController : Controller
	{
		private readonly IFetcherService fetcherService;
		public FetcherController(IFetcherService fetcherService)
		{
			this.fetcherService = fetcherService;
		}

		[HttpPost]
		public async Task<ActionResult<Fetcher.Abstractions.Dtos.Subject>> Fetch([FromBody] FetchVM fetchVM)
		{
			var subject = await fetcherService.Fetch(fetchVM.Url, true);
			return Ok(subject);
		}
	}
}
