using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using MintPlayer.Data.Services;

namespace MintPlayer.Web.Server.Controllers.Web.V1
{
	[Controller]
	[Route("web/v1/[controller]")]
	public class SongController : Controller
	{
		private ISongService songService;
		public SongController(ISongService songService)
		{
			this.songService = songService;
		}

		// POST: web/Song/page
		[HttpPost("page", Name = "web-v1-song-page")]
		public async Task<ActionResult<Pagination.PaginationResponse<Song>>> PageSongs([FromBody] Pagination.PaginationRequest<Song> request)
		{
			var songs = await songService.PageSongs(request);
			return Ok(songs);
		}

		// GET: web/Song
		[HttpGet(Name = "web-v1-song-list")]
		public async Task<ActionResult<IEnumerable<Song>>> Get([FromHeader]bool include_relations = false)
		{
			var songs = await songService.GetSongs(include_relations, false);
			return Ok(songs);
		}
		// GET: web/Song/5
		[HttpGet("{id}", Name = "web-v1-song-get", Order = 1)]
		public async Task<ActionResult<Song>> Get(int id, [FromHeader]bool include_relations = false)
		{
			var song = await songService.GetSong(id, include_relations, false);
			return Ok(song);
		}
		// GET: web/Song/5/lyrics
		[HttpGet("{id}/lyrics", Name = "web-v1-song-lyrics")]
		public async Task<ActionResult<Lyrics>> Lyrics(int id)
		{
			var song = await songService.GetSong(id, true, false);
			return Ok(song.Lyrics);
		}
		// POST: web/Song
		[HttpPost(Name = "web-v1-song-create")]
		[Authorize]
		public async Task<ActionResult<Song>> Post([FromBody] Song song)
		{
			var new_song = await songService.InsertSong(song);
			return Ok(new_song);
		}
		// PUT: web/Song/5
		[HttpPut("{id}", Name = "web-v1-song-update")]
		[Authorize]
		public async Task<ActionResult<Song>> Put(int id, [FromBody] Song song)
		{
			var updated_song = await songService.UpdateSong(song);
			return Ok(updated_song);
		}
		// PUT: web/Song/5/timeline
		[HttpPut("{id}/timeline", Name = "web-v1-song-timeline-update")]
		[Authorize]
		public async Task<ActionResult<Song>> UpdateTimeline(int id, [FromBody] Song song)
		{
			await songService.UpdateTimeline(song);
			return Ok();
		}
		// DELETE: web/Song/5
		[HttpDelete("{id}", Name = "web-v1-song-delete")]
		[Authorize]
		public async Task<ActionResult> Delete(int id)
		{
			await songService.DeleteSong(id);
			return Ok();
		}
	}
}