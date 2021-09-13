using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using MintPlayer.Data.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Data;

namespace MintPlayer.Web.Server.Controllers.Api
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ArtistController : Controller
    {
        private IArtistService artistService;
        public ArtistController(IArtistService artistService)
        {
            this.artistService = artistService;
        }

        /// <summary>Page through the artists in the database.</summary>
        /// <param name="request">Object containing the pagination information.</param>
        /// <returns>A slice of the artists.</returns>
        [EnableCors(CorsPolicies.AllowDatatables)]
        [HttpPost("page", Name = "api-artist-page")]
        public async Task<ActionResult<Pagination.PaginationResponse<Artist>>> PageArtists([FromBody] Pagination.PaginationRequest<Artist> request)
        {
            var artists = await artistService.PageArtists(request);
            return Ok(artists);
        }

        /// <summary>Get the artists in the database.</summary>
        /// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
        /// <returns>A list of the artists.</returns>
        [HttpGet(Name = "api-artist-list")]
        public async Task<ActionResult<IEnumerable<Artist>>> Get([FromHeader] bool include_relations = false)
        {
            var artists = await artistService.GetArtists(include_relations);
            return Ok(artists.ToList());
        }

        /// <summary>Get a specific artist from the database.</summary>
        /// <param name="id">The id of the artist to retrieve.</param>
        /// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
        /// <returns>The artist with the specified id.</returns>
        [HttpGet("{id}", Name = "api-artist-get", Order = 1)]
        public async Task<ActionResult<Artist>> Get(int id, [FromHeader] bool include_relations = false)
        {
            var artist = await artistService.GetArtist(id, include_relations);

            if (artist == null) return NotFound();
            else return Ok(artist);
        }

        /// <summary>Get the favorite artists for the current user.</summary>
        /// <param name="request">Object containing the pagination information.</param>
        /// <returns>A slice of the favorite artists for the current user.</returns>
        [HttpPost("favorite", Name = "api-artist-favorite-page")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Pagination.PaginationResponse<Artist>>> PageFavoriteArtists([FromBody] Pagination.PaginationRequest<Artist> request)
        {
            var artists = await artistService.PageLikedArtists(request);
            return Ok(artists);
        }

        /// <summary>Get the favorite artists for the current user.</summary>
        /// <returns>The favorite artists for the current user.</returns>
        [HttpGet("favorite", Name = "api-artist-favorite")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<Artist>>> FavoriteArtists()
        {
            var artists = await artistService.GetLikedArtists();
            return Ok(artists);
        }

        /// <summary>Creates a new artist in the database.</summary>
        /// <param name="artist">Artist to be inserted in the database.</param>
        /// <returns>The newly created artist with the newly assigned id.</returns>
        [HttpPost(Name = "api-artist-create")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Artist>> Post([FromBody] Artist artist)
        {
            var new_artist = await artistService.InsertArtist(artist);
            return Ok(new_artist);
        }

        /// <summary>Updates an artist in the database.</summary>
        /// <param name="artist">New artist information.</param>
        /// <returns>The updated artist.</returns>
        [HttpPut("{id}", Name = "api-artist-update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Artist>> Put([FromBody] Artist artist)
        {
            try
            {
                var updated_artist = await artistService.UpdateArtist(artist);
                return Ok(updated_artist);
            }
            catch (Data.Exceptions.NotFoundException notFoundEx)
            {
                return NotFound();
            }
            catch (Data.Exceptions.ConcurrencyException<Artist> concurrencyEx)
            {
				return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status409Conflict, concurrencyEx.DatabaseValue);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        /// <summary>Deletes an artist from the database.</summary>
        /// <param name="id">Id of the artist to delete.</param>
        [HttpDelete("{id}", Name = "api-artist-delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await artistService.DeleteArtist(id);
                return Ok();
            }
            catch (Data.Exceptions.NotFoundException notFoundEx)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
