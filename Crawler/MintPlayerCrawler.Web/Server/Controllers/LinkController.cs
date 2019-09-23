using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MintPlayerCrawler.Data.Dtos;
using MintPlayerCrawler.Data.Repositories.Interfaces;
using MintPlayerCrawler.Web.Server.ViewModels.Link;

namespace MintPlayerCrawler.Web.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinkController : Controller
    {
        private ILinkRepository linkRepository;
        public LinkController(ILinkRepository linkRepository)
        {
            this.linkRepository = linkRepository;
        }

        [HttpGet]
        public IEnumerable<Link> Index()
        {
            var links = linkRepository.GetLinks();
            return links.ToList();
        }

        [HttpGet("{id}")]
        public Link Get(int id)
        {
            var link = linkRepository.GetLink(id);
            return link;
        }

        [HttpPost]
        public async Task<Link> Post([FromBody]LinkCreateVM linkCreateVM)
        {
            var link = await linkRepository.InsertLink(linkCreateVM.Link);
            return link;
        }

        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody]LinkUpdateVM linkUpdateVM)
        {
            await linkRepository.UpdateLink(linkUpdateVM.Link);
            await linkRepository.SaveChangesAsync();
        }
    }
}