using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Data.Services;

namespace MintPlayer.Web.Server.Controllers.Amp
{
    [Controller]
    [Route("amp/[controller]")]
    public class SongController : Controller
    {
        private ISongService songService;
        public SongController(ISongService songService)
        {
            this.songService = songService;
        }

        // GET: amp/Song/5
        [HttpGet("{id}", Name = "amp-song-show", Order = 1)]
        public async Task<ActionResult> Show(int id)
        {
            var song = await songService.GetSong(id, true, false);

            if (song == null) return NotFound();
            else return View(song);
        }
    }
}
