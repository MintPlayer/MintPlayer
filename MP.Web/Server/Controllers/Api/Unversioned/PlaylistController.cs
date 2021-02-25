using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Services;

namespace MintPlayer.Web.Server.Controllers.Api
{
    [Controller]
    [Route("api/[controller]")]
    public class PlaylistController : Controller
    {
        private readonly IPlaylistService playlistService;
        public PlaylistController(IPlaylistService playlistService)
        {
            this.playlistService = playlistService;
        }

        // POST: api/Playlist/page
        [HttpPost("my/page", Name = "api-playlist-my-page")]
        [Authorize]
        public async Task<ActionResult<Pagination.PaginationResponse<Playlist>>> PageMyPlaylists([FromBody] Pagination.PaginationRequest<Playlist> request)
        {
            var playlists = await playlistService.PagePlaylists(request, Data.Enums.ePlaylistScope.My);
            return Ok(playlists);
        }

        // POST: api/Playlist/page
        [HttpPost("public/page", Name = "api-playlist-public-page")]
        public async Task<ActionResult<Pagination.PaginationResponse<Playlist>>> PagePublicPlaylists([FromBody] Pagination.PaginationRequest<Playlist> request)
        {
            var playlists = await playlistService.PagePlaylists(request, Data.Enums.ePlaylistScope.Public);
            return Ok(playlists);
        }

        // GET api/Playlist
        [HttpGet("my", Name = "api-playlist-my-list")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Playlist>>> GetMyPlaylists([FromHeader] bool include_relations = false)
        {
            var playlists = await playlistService.GetPlaylists(Data.Enums.ePlaylistScope.My, include_relations);
            return Ok(playlists);
        }

        // GET api/Playlist
        [HttpGet("public", Name = "api-playlist-public-list")]
        public async Task<ActionResult<IEnumerable<Playlist>>> GetPublicPlaylists([FromHeader] bool include_relations = false)
        {
            var playlists = await playlistService.GetPlaylists(Data.Enums.ePlaylistScope.Public, include_relations);
            return Ok(playlists);
        }

        // GET api/5
        [HttpGet("{id}", Name = "api-playlist-get", Order = 1)]
        public async Task<ActionResult<Playlist>> Get(int id, [FromHeader]bool include_relations = false)
        {
            try
            {
                var playlist = await playlistService.GetPlaylist(id, include_relations);

                if (playlist == null) return NotFound();
                else return Ok(playlist);
            }
            catch (Data.Exceptions.ForbiddenException forbiddenEx)
            {
                return Forbid();
            }
            catch (UnauthorizedAccessException unauthEx)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        // POST api/Playlist
        [HttpPost(Name = "api-playlist-create")]
        [Authorize]
        public async Task<ActionResult<Playlist>> Post([FromBody] Playlist playlist)
        {
            var new_playlist = await playlistService.InsertPlaylist(playlist);
            return Ok(new_playlist);
        }

        // PUT api/Playlist/5
        [HttpPut("{id}", Name = "api-playlist-update")]
        [Authorize]
        public async Task<ActionResult<Playlist>> Put(int id, [FromBody] Playlist playlist)
        {
            var updated_playlist = await playlistService.UpdatePlaylist(playlist);
            return Ok(updated_playlist);
        }

        // DELETE api/Playlist/5
        [HttpDelete("{id}", Name = "api-playlist-delete")]
        public async Task<ActionResult> Delete(int id)
        {
            await playlistService.DeletePlaylist(id);
            return Ok();
        }
    }
}