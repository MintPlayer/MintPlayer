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

namespace MintPlayer.Web.Server.Controllers.Api
{
	[ApiController]
	//[EnableCors("AllowPage")]
	[Route("api/[controller]")]
	public class SongController : Controller
	{
		private ISongService songService;
		public SongController(ISongService songService)
		{
			this.songService = songService;
		}

		// POST: api/Song/page
		//[EnableCors(CorsPolicies.AllowDatatables)]
		[HttpPost("page", Name = "api-song-page")]
		public async Task<ActionResult<Pagination.PaginationResponse<Song>>> PageSongs([FromBody] Pagination.PaginationRequest<Song> request)
		{
			var songs = await songService.PageSongs(request);
			return Ok(songs);
		}

		// GET: api/Song
		[HttpGet(Name = "api-song-list")]
		public async Task<ActionResult<IEnumerable<Song>>> Get([FromHeader]bool include_relations = false)
		{
			var songs = await songService.GetSongs(include_relations, false);
			return Ok(songs);
		}

		// GET: api/Song/5
		[HttpGet("{id}", Name = "api-song-get", Order = 1)]
		public async Task<ActionResult<Song>> Get(int id, [FromHeader]bool include_relations = false)
		{
			var song = await songService.GetSong(id, include_relations, false);

			if (song == null) return NotFound();
			else return Ok(song);
		}

		// GET: api/Song/5/lyrics
		[HttpGet("{id}/lyrics", Name = "api-song-lyrics")]
		public async Task<ActionResult<Lyrics>> Lyrics(int id)
		{
			var song = await songService.GetSong(id, true, false);
			return Ok(song.Lyrics);
		}

		// POST: api/Song/favorite
		[HttpPost("favorite", Name = "api-song-favorite-page")]
		[Authorize]
		public async Task<ActionResult<Pagination.PaginationResponse<Artist>>> PageFavoriteSongs([FromBody] Pagination.PaginationRequest<Song> request)
		{
			var songs = await songService.PageLikedSongs(request);
			return Ok(songs);
		}

		// GET: api/Song/favorite
		[HttpGet("favorite", Name = "api-song-favorite")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<Song>>> FavoriteSongs()
		{
			var songs = await songService.GetLikedSongs();
			return Ok(songs);
		}

		// POST: api/Song
		[HttpPost(Name = "api-song-create")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<ActionResult<Song>> Post([FromBody] Song song)
		{
			var new_song = await songService.InsertSong(song);
			return Ok(new_song);
		}

		// PUT: api/Song/5
		[HttpPut("{id}", Name = "api-song-update")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<ActionResult<Song>> Put(int id, [FromBody] Song song)
		{
			var updated_song = await songService.UpdateSong(song);
			return Ok(updated_song);
		}

		// PUT: api/Song/5/timeline
		[HttpPut("{id}/timeline", Name = "api-song-timeline-update")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<ActionResult<Song>> UpdateTimeline(int id, [FromBody] Song song)
		{
			await songService.UpdateTimeline(song);
			return Ok();
		}

		// DELETE: api/ApiWithActions/5
		[HttpDelete("{id}", Name = "api-song-delete")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<ActionResult> Delete(int id)
		{
			await songService.DeleteSong(id);
			return Ok();
		}
	}
}