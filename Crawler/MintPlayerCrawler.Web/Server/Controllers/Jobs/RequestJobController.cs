using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MintPlayerCrawler.Data.Dtos.Jobs;
using MintPlayerCrawler.Data.Repositories.Interfaces;
using MintPlayerCrawler.Web.Server.ViewModels.Jobs.RequestJob;

namespace MintPlayerCrawler.Web.Server.Controllers.Jobs
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestJobController : Controller
    {
        private IRequestJobRepository requestJobRepository;
        public RequestJobController(IRequestJobRepository requestJobRepository)
        {
            this.requestJobRepository = requestJobRepository;
        }

        [HttpPost]
        public async Task<RequestJob> Post([FromBody]RequestJobCreateVM requestJobCreateVM)
        {
            var request_job = await requestJobRepository.CreateRequestJob(requestJobCreateVM.Job);
            return request_job;
        }
    }
}