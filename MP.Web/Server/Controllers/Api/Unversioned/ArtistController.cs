using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using MintPlayer.Data.Services;

namespace MintPlayer.Web.Server.Controllers.Api
{
	[ApiController]
	[Route("api/[controller]")]
	public class ArtistController : Controller
	{
		private IArtistService artistService;
		public ArtistController(IArtistService artistService)
		{
			this.artistService = artistService;
		}

		// POST: api/Artist/page
		[HttpPost("page", Name = "api-artist-page")]
		public async Task<ActionResult<Pagination.PaginationResponse<Artist>>> PageArtists([FromBody] Pagination.PaginationRequest<Artist> request)
		{
			var artists = await artistService.PageArtists(request);
			return Ok(artists);
		}

		// GET: api/Artist
		[HttpGet(Name = "api-artist-list")]
		public async Task<ActionResult<IEnumerable<Artist>>> Get([FromHeader]bool include_relations = false)
		{
			var artists = await artistService.GetArtists(include_relations, false);
			return Ok(artists.ToList());
		}

		// GET: api/Artist/5
		[HttpGet("{id}", Name = "api-artist-get", Order = 1)]
		public async Task<ActionResult<Artist>> Get(int id, [FromHeader]bool include_relations = false)
		{
			var artist = await artistService.GetArtist(id, include_relations, false);

			if (artist == null) return NotFound();
			else return Ok(artist);
		}

		// POST: api/Artist/favorite
		[HttpPost("favorite", Name = "api-artist-favorite-page")]
		[Authorize]
		public async Task<ActionResult<Pagination.PaginationResponse<Artist>>> PageFavoriteArtists([FromBody] Pagination.PaginationRequest<Artist> request)
		{
			var artists = await artistService.PageLikedArtists(request);
			return Ok(artists);
		}

		// GET: api/Artist/favorite
		[HttpGet("favorite", Name = "api-artist-favorite")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<Artist>>> FavoriteArtists()
		{
			var artists = await artistService.GetLikedArtists();
			return Ok(artists);
		}

		// POST: api/Artist
		[HttpPost(Name = "api-artist-create")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<ActionResult<Artist>> Post([FromBody] Artist artist)
		{
			var new_artist = await artistService.InsertArtist(artist);
			return Ok(new_artist);
		}

		// PUT: api/Artist/5
		[HttpPut("{id}", Name = "api-artist-update")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<ActionResult<Artist>> Put(int id, [FromBody] Artist artist)
		{
			var updated_artist = await artistService.UpdateArtist(artist);
			return Ok(updated_artist);
		}

		// DELETE: api/Artist/5
		[HttpDelete("{id}", Name = "api-artist-delete")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<ActionResult> Delete(int id)
		{
			await artistService.DeleteArtist(id);
			return Ok();
		}
	}
}