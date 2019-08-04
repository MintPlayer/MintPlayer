using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Data.Dtos;
using MintPlayer.Data.Repositories.Interfaces;
using MintPlayer.Web.ViewModels.Song;

namespace MintPlayer.Web.Controllers
{
    [Route("api/[controller]")]
    public class SongController : Controller
    {
        private ISongRepository songRepository;
        public SongController(ISongRepository songRepository)
        {
            this.songRepository = songRepository;
        }

        // GET: api/Song
        [HttpGet]
        public IEnumerable<Song> Get([FromHeader]bool include_relations = false)
        {
            var songs = songRepository.GetSongs(include_relations);
            return songs.ToList();
        }
        // GET: api/Song/5
        [HttpGet("{id}", Order = 1)]
        public Song Get(int id, [FromHeader]bool include_relations = false)
        {
            var song = songRepository.GetSong(id, include_relations);
            return song;
        }
        // POST: api/Song
        [HttpPost]
        [Authorize]
        public async Task<Song> Post([FromBody] SongCreateVM songCreateVM)
        {
            var song = await songRepository.InsertSong(songCreateVM.Song);
            return song;
        }
        // PUT: api/Song/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task Put(int id, [FromBody] SongUpdateVM songUpdateVM)
        {
            await songRepository.UpdateSong(songUpdateVM.Song);
            await songRepository.SaveChangesAsync();
        }
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task Delete(int id)
        {
            await songRepository.DeleteSong(id);
            await songRepository.SaveChangesAsync();
        }
    }
}