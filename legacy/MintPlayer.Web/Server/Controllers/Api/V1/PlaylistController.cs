using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MintPlayer.Data.Exceptions;
using MintPlayer.Data.Abstractions.Services;

namespace MintPlayer.Web.Server.Controllers.Api
{
	[Controller]
    [Route("api/v1/[controller]")]
    public class PlaylistController : Controller
    {
        private readonly IPlaylistService playlistService;
        public PlaylistController(IPlaylistService playlistService)
        {
            this.playlistService = playlistService;
        }

        /// <summary>Page through the current user's playlists in the database.</summary>
        /// <param name="request">Object containing the pagination information.</param>
        /// <returns>A slice of the playlists for the current user.</returns>
        [HttpPost("my/page", Name = "api-playlist-my-page")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Pagination.PaginationResponse<Playlist>>> PageMyPlaylists([FromBody] Pagination.PaginationRequest<Playlist> request)
        {
            var playlists = await playlistService.PagePlaylists(request, Data.Abstractions.Enums.EPlaylistScope.My);
            return Ok(playlists);
        }

        /// <summary>Page through the public playlists in the database.</summary>
        /// <param name="request">Object containing the pagination information.</param>
        /// <returns>A slice of the public playlists.</returns>
        [HttpPost("public/page", Name = "api-playlist-public-page")]
        public async Task<ActionResult<Pagination.PaginationResponse<Playlist>>> PagePublicPlaylists([FromBody] Pagination.PaginationRequest<Playlist> request)
        {
            var playlists = await playlistService.PagePlaylists(request, Data.Abstractions.Enums.EPlaylistScope.Public);
            return Ok(playlists);
        }

        /// <summary>Get the playlists for the current user from the database.</summary>
        /// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
        /// <returns>The playlists for the current user.</returns>
        [HttpGet("my", Name = "api-playlist-my-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<Playlist>>> GetMyPlaylists([FromHeader] bool include_relations = false)
        {
            var playlists = await playlistService.GetPlaylists(Data.Abstractions.Enums.EPlaylistScope.My, include_relations);
            return Ok(playlists);
        }

        /// <summary>Get the public playlists from the database.</summary>
        /// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
        /// <returns>A list of the public playlists.</returns>
        [HttpGet("public", Name = "api-playlist-public-list")]
        public async Task<ActionResult<IEnumerable<Playlist>>> GetPublicPlaylists([FromHeader] bool include_relations = false)
        {
            var playlists = await playlistService.GetPlaylists(Data.Abstractions.Enums.EPlaylistScope.Public, include_relations);
            return Ok(playlists);
        }

        /// <summary>Get a specific playlist from the database.</summary>
        /// <param name="id">The id of the playlist to retrieve.</param>
        /// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
        /// <returns>The playlist with the specified id.</returns>
        [HttpGet("{id}", Name = "api-playlist-get", Order = 1)]
        public async Task<ActionResult<Playlist>> GetPlaylist(int id, [FromHeader] bool include_relations = false)
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

        /// <summary>Creates a new playlist in the database.</summary>
        /// <param name="playlist">Playlist to be inserted in the database.</param>
        /// <returns>The newly created playlist with the newly assigned id.</returns>
        [HttpPost(Name = "api-playlist-create")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Playlist>> Post([FromBody] Playlist playlist)
        {
            var new_playlist = await playlistService.InsertPlaylist(playlist);
            return Ok(new_playlist);
        }

        /// <summary>Updates a playlist in the database.</summary>
        /// <param name="playlist">New playlist information.</param>
        /// <returns>The updated playlist.</returns>
        [HttpPut("{id}", Name = "api-playlist-update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Playlist>> Put([FromBody] Playlist playlist)
        {
            var updated_playlist = await playlistService.UpdatePlaylist(playlist);
            return Ok(updated_playlist);
        }

        /// <summary>Deletes a playlist from the database.</summary>
        /// <param name="id">Id of the playlist to delete.</param>
        [HttpDelete("{id}", Name = "api-playlist-delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await playlistService.DeletePlaylist(id);
                return Ok();
            }
            catch (NotFoundException notFoundEx)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
