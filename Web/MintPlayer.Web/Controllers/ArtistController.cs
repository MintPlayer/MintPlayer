using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MintPlayer.Data.Dtos;
using MintPlayer.Data.Repositories.Interfaces;
using MintPlayer.Web.ViewModels.Artist;

namespace MintPlayer.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistController : Controller
    {
        private IArtistRepository artistRepository;
        public ArtistController(IArtistRepository artistRepository)
        {
            this.artistRepository = artistRepository;
        }

        // GET: api/Artist
        [HttpGet]
        public IEnumerable<Artist> Get([FromHeader]bool include_relations = false)
        {
            var artists = artistRepository.GetArtists(include_relations);
            return artists.ToList();
        }

        // GET: api/Artist/5
        [HttpGet("{id}", Order = 1)]
        public Artist Get(int id, [FromHeader]bool include_relations = false)
        {
            var artist = artistRepository.GetArtist(id, include_relations);
            return artist;
        }

        // POST: api/Artist
        [HttpPost]
        [Authorize]
        public async Task<Artist> Post([FromBody] ArtistCreateVM artistCreateVM)
        {
            var artist = await artistRepository.InsertArtist(artistCreateVM.Artist);
            return artist;
        }

        // PUT: api/Artist/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task Put(int id, [FromBody] ArtistUpdateVM artistUpdateVM)
        {
            await artistRepository.UpdateArtist(artistUpdateVM.Artist);
            await artistRepository.SaveChangesAsync();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task Delete(int id)
        {
            await artistRepository.DeleteArtist(id);
            await artistRepository.SaveChangesAsync();
        }
    }
}