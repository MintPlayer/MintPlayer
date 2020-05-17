using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Services;

namespace MintPlayer.Web.Server.Controllers.Web.V2
{
    [Controller]
    [Route("web/v2/[controller]")]
    public class PlaylistController : Controller
    {
        private readonly IPlaylistService playlistService;
        public PlaylistController(IPlaylistService playlistService)
        {
            this.playlistService = playlistService;
        }

        // POST: web/Playlist/page
        [HttpPost("page", Name = "web-v2-playlist-page")]
        [Authorize]
        public async Task<ActionResult<Pagination.PaginationResponse<Playlist>>> PagePlaylists([FromBody] Pagination.PaginationRequest<Playlist> request)
        {
            var playlists = await playlistService.PagePlaylists(request);
            return Ok(playlists);
        }

        // GET web/Playlist
        [HttpGet(Name = "web-v2-playlist-list")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Playlist>>> Get([FromHeader] bool include_relations = false)
        {
            var playlists = await playlistService.GetPlaylists(include_relations);
            return Ok(playlists);
        }

        // GET web/Playlist/5
        [HttpGet("{id}", Name = "web-v2-playlist-get", Order = 1)]
        [Authorize]
        public async Task<ActionResult<Playlist>> Get(int id, [FromHeader]bool include_relations = false)
        {
            var playlist = await playlistService.GetPlaylist(id, include_relations);

            if (playlist == null) return NotFound();
            else return Ok(playlist);
        }

        // POST web/Playlist
        [HttpPost(Name = "web-v2-playlist-create")]
        [Authorize]
        public async Task<ActionResult<Playlist>> Post([FromBody] Playlist playlist)
        {
            try
            {
                var new_playlist = await playlistService.InsertPlaylist(playlist);
                return Ok(new_playlist);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        // PUT web/Playlist/5
        [HttpPut("{id}", Name = "web-v2-playlist-update")]
        [Authorize]
        public async Task<ActionResult<Playlist>> Put(int id, [FromBody] Playlist playlist)
        {
            var updated_playlist = await playlistService.UpdatePlaylist(playlist);
            return Ok(updated_playlist);
        }

        // DELETE web/Playlist/5
        [HttpDelete("{id}", Name = "web-v2-playlist-delete")]
        public async Task<ActionResult> Delete(int id)
        {
            await playlistService.DeletePlaylist(id);
            return Ok();
        }
    }
}