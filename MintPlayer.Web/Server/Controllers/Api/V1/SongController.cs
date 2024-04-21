using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MintPlayer.Data.Abstractions.Services;

namespace MintPlayer.Web.Server.Controllers.Api;

[ApiController]
[Route("api/v1/[controller]")]
public class SongController : Controller
{
	private ISongService songService;
	public SongController(ISongService songService)
	{
		this.songService = songService;
	}

	/// <summary>Page through the songs in the database.</summary>
	/// <param name="request">Object containing the pagination information.</param>
	/// <returns>A slice of the songs.</returns>
	[EnableCors(CorsPolicies.AllowDatatables)]
	[HttpPost("page", Name = "api-song-page")]
	public async Task<ActionResult<Pagination.PaginationResponse<Song>>> PageSongs([FromBody] Pagination.PaginationRequest<Song> request)
	{
		var songs = await songService.PageSongs(request);
		return Ok(songs);
	}

	/// <summary>Get the songs in the database.</summary>
	/// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
	/// <returns>A list of the songs.</returns>
	[HttpGet(Name = "api-song-list")]
	public async Task<ActionResult<IEnumerable<Song>>> Get([FromHeader] bool include_relations = false)
	{
		var songs = await songService.GetSongs(include_relations);
		return Ok(songs);
	}

	/// <summary>Get a specific song from the database.</summary>
	/// <param name="id">The id of the song to retrieve.</param>
	/// <param name="include_relations">Specifies whether the related entities should be included in the response.</param>
	/// <returns>The song with the specified id.</returns>
	[HttpGet("{id}", Name = "api-song-get", Order = 1)]
	public async Task<ActionResult<Song>> Get(int id, [FromHeader] bool include_relations = false)
	{
		var song = await songService.GetSong(id, include_relations);

		if (song == null) return NotFound();
		else return Ok(song);
	}

	/// <summary>Get the lyrics as plain text for a specific song.</summary>
	/// <param name="id">The id of the song.</param>
	/// <returns>The lyrics for the song with the specified id.</returns>
	[HttpGet("{id}/lyrics", Name = "api-song-lyrics")]
	public async Task<ActionResult<Lyrics>> Lyrics(int id)
	{
		var song = await songService.GetSong(id, true);
		return Ok(song.Lyrics);
	}

	/// <summary>Get the favorite songs for the current user.</summary>
	/// <param name="request">Object containing the pagination information.</param>
	/// <returns>A slice of the favorite songs for the current user.</returns>
	[HttpPost("favorite", Name = "api-song-favorite-page")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<Pagination.PaginationResponse<Artist>>> PageFavoriteSongs([FromBody] Pagination.PaginationRequest<Song> request)
	{
		var songs = await songService.PageLikedSongs(request);
		return Ok(songs);
	}

	/// <summary>Get the favorite songs for the current user.</summary>
	/// <returns>The favorite songs for the current user.</returns>
	[HttpGet("favorite", Name = "api-song-favorite")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<IEnumerable<Song>>> FavoriteSongs()
	{
		var songs = await songService.GetLikedSongs();
		return Ok(songs);
	}

	/// <summary>Creates a new song in the database.</summary>
	/// <param name="song">Song to be inserted in the database.</param>
	/// <returns>The newly created song with the newly assigned id.</returns>
	[HttpPost(Name = "api-song-create")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<Song>> Post([FromBody] Song song)
	{
		var new_song = await songService.InsertSong(song);
		return Ok(new_song);
	}

	/// <summary>Updates a song in the database.</summary>
	/// <param name="song">New song information.</param>
	/// <returns>The updated song.</returns>
	[HttpPut("{id}", Name = "api-song-update")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<Song>> Put([FromBody] Song song)
	{
		try
		{
			var updated_song = await songService.UpdateSong(song);
			return Ok(updated_song);
		}
		catch (Data.Exceptions.NotFoundException notFoundEx)
		{
			return NotFound();
		}
		catch (Data.Exceptions.ConcurrencyException<Song> concurrencyEx)
		{
			return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status409Conflict, concurrencyEx.DatabaseValue);
		}
		catch (Exception ex)
		{
			return StatusCode(500);
		}
	}

	/// <summary>Updates the timeline for a specific song.</summary>
	/// <param name="id">Id of the song to update.</param>
	/// <param name="song">Song to update the timeline for.</param>
	[HttpPut("{id}/timeline", Name = "api-song-timeline-update")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult> UpdateTimeline(int id, [FromBody] Song song)
	{
		await songService.UpdateTimeline(song);
		return Ok();
	}

	/// <summary>Deletes a song from the database.</summary>
	/// <param name="id">The id of the song to delete.</param>
	[HttpDelete("{id}", Name = "api-song-delete")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult> Delete(int id)
	{
		try
		{
			await songService.DeleteSong(id);
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
