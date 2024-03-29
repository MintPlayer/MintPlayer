﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Abstractions.Services;

namespace MintPlayer.Web.Server.Controllers.Web.V3
{
	[Controller]
	[Route("web/v3/[controller]")]
	public class ArtistController : Controller
	{
		private IArtistService artistService;
		public ArtistController(IArtistService artistService)
		{
			this.artistService = artistService;
		}

		// POST: web/Artist/page
		[HttpPost("page", Name = "web-v3-artist-page")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<Pagination.PaginationResponse<Artist>>> PageArtists([FromBody] Pagination.PaginationRequest<Artist> request)
		{
			var artists = await artistService.PageArtists(request);
			return Ok(artists);
		}

		// GET: web/Artist
		[HttpGet(Name = "web-v3-artist-list")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<IEnumerable<Artist>>> Get([FromHeader]bool include_relations = false)
		{
			var artists = await artistService.GetArtists(include_relations);
			return Ok(artists);
		}

		// GET: web/Artist/5
		[HttpGet("{id}", Name = "web-v3-artist-get", Order = 1)]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<Artist>> Get(int id, [FromHeader]bool include_relations = false)
		{
			var artist = await artistService.GetArtist(id, include_relations);

			if (artist == null) return NotFound();
			return Ok(artist);
		}

		// POST: web/Artist/favorite
		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPost("favorite", Name = "web-v3-artist-favorite-page")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<Pagination.PaginationResponse<Artist>>> PageFavoriteArtists([FromBody] Pagination.PaginationRequest<Artist> request)
		{
			var artists = await artistService.PageLikedArtists(request);
			return Ok(artists);
		}

		// GET: web/Artist/favorite
		[Authorize]
		[HttpGet("favorite", Name = "web-v3-artist-favorite")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<IEnumerable<Artist>>> FavoriteArtists()
		{
			var artists = await artistService.GetLikedArtists();
			return Ok(artists);
		}

		// POST: web/Artist
		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPost(Name = "web-v3-artist-create")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<Artist>> Post([FromBody] Artist artist)
		{
			var new_artist = await artistService.InsertArtist(artist);
			return Ok(new_artist);
		}

		// PUT: web/Artist/5
		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPut("{id}", Name = "web-v3-artist-update")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<Artist>> Put(int id, [FromBody] Artist artist)
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

		// DELETE: web/Artist/5
		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpDelete("{id}", Name = "web-v3-artist-delete")]
		[ApiExplorerSettings(IgnoreApi = true)]
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
