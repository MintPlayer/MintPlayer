using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using MintPlayer.Data.Services;

namespace MintPlayer.Web.Server.Controllers.Web.V3
{
	[Controller]
	[Route("web/v3/[controller]")]
	public class SongController : Controller
	{
		private ISongService songService;
		public SongController(ISongService songService)
		{
			this.songService = songService;
		}

		// POST: web/Song/page
		[HttpPost("page", Name = "web-v3-song-page")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<Pagination.PaginationResponse<Song>>> PageSongs([FromBody] Pagination.PaginationRequest<Song> request)
		{
			var songs = await songService.PageSongs(request);
			return Ok(songs);
		}

		// GET: web/Song
		[HttpGet(Name = "web-v3-song-list")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<IEnumerable<Song>>> Get([FromHeader]bool include_relations = false)
		{
			var songs = await songService.GetSongs(include_relations, false);
			return Ok(songs);
		}

		// GET: web/Song/5
		[HttpGet("{id}", Name = "web-v3-song-get", Order = 1)]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<Song>> Get(int id, [FromHeader]bool include_relations = false)
		{
			var song = await songService.GetSong(id, include_relations, false);

			if (song == null) return NotFound();
			else return Ok(song);
		}

		// GET: web/Song/5/lyrics
		[HttpGet("{id}/lyrics", Name = "web-v3-song-lyrics")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<Lyrics>> Lyrics(int id)
		{
			var song = await songService.GetSong(id, true, false);
			return Ok(song.Lyrics);
		}

		// POST: web/Song/favorite
		[Authorize]
		[HttpPost("favorite", Name = "web-v3-song-favorite-page")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<Pagination.PaginationResponse<Artist>>> PageFavoriteSongs([FromBody] Pagination.PaginationRequest<Song> request)
		{
			var songs = await songService.PageLikedSongs(request);
			return Ok(songs);
		}

		// GET: web/Song/favorite
		[Authorize]
		[HttpGet("favorite", Name = "web-v3-song-favorite")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<IEnumerable<Song>>> FavoriteSongs()
		{
			var songs = await songService.GetLikedSongs();
			return Ok(songs);
		}

		// POST: web/Song
		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPost(Name = "web-v3-song-create")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<Song>> Post([FromBody] Song song)
		{
			var new_song = await songService.InsertSong(song);
			return Ok(new_song);
		}
		// PUT: web/Song/5
		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPut("{id}", Name = "web-v3-song-update")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<Song>> Put(int id, [FromBody] Song song)
		{
			var updated_song = await songService.UpdateSong(song);
			return Ok(updated_song);
		}
		// PUT: web/Song/5/timeline
		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPut("{id}/timeline", Name = "web-v3-song-timeline-update")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<Song>> UpdateTimeline(int id, [FromBody] Song song)
		{
			await songService.UpdateTimeline(song);
			return Ok();
		}
		// DELETE: web/Song/5
		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpDelete("{id}", Name = "web-v3-song-delete")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult> Delete(int id)
		{
			await songService.DeleteSong(id);
			return Ok();
		}
	}
}