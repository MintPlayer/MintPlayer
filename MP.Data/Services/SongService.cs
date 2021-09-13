using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace MintPlayer.Data.Services
{
	public interface ISongService
	{
		Task<Pagination.PaginationResponse<Song>> PageSongs(Pagination.PaginationRequest<Song> request);
		Task<IEnumerable<Song>> GetSongs(bool include_relations);
		Task<Song> GetSong(int id, bool include_relations);
		Task<Pagination.PaginationResponse<Song>> PageLikedSongs(Pagination.PaginationRequest<Song> request);
		Task<IEnumerable<Song>> GetLikedSongs();
		Task<Song> InsertSong(Song song);
		Task<Song> UpdateSong(Song song);
		Task UpdateTimeline(Song song);
		Task DeleteSong(int song_id);
	}

	internal class SongService : ISongService
	{
		private readonly ISongRepository songRepository;
		private readonly IMediumRepository mediumRepository;
		private readonly UserManager<Entities.User> userManager;
		private readonly IHttpContextAccessor httpContextAccessor;
		public SongService(ISongRepository songRepository, IMediumRepository mediumRepository, UserManager<Entities.User> userManager, IHttpContextAccessor httpContextAccessor)
		{
			this.songRepository = songRepository;
			this.mediumRepository = mediumRepository;
			this.userManager = userManager;
			this.httpContextAccessor = httpContextAccessor;
		}

		public async Task<Pagination.PaginationResponse<Song>> PageSongs(Pagination.PaginationRequest<Song> request)
		{
			var songs = await songRepository.PageSongs(request);
			return songs;
		}

		public async Task<IEnumerable<Song>> GetSongs(bool include_relations)
		{
			var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
			var isAdmin = user == null ? false : await userManager.IsInRoleAsync(user, "Administrator");
			var songs = await songRepository.GetSongs(include_relations, isAdmin);
			return songs;
		}

		public async Task<Song> GetSong(int id, bool include_relations)
		{
			var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
			var isAdmin = user == null ? false : await userManager.IsInRoleAsync(user, "Administrator");
			var song = await songRepository.GetSong(id, include_relations, isAdmin);
			return song;
		}

		public Task<Pagination.PaginationResponse<Song>> PageLikedSongs(Pagination.PaginationRequest<Song> request)
		{
			return songRepository.PageLikedSongs(request);
		}

		public Task<IEnumerable<Song>> GetLikedSongs()
		{
			return songRepository.GetLikedSongs();
		}

		public async Task<Song> InsertSong(Song song)
		{
			var new_song = await songRepository.InsertSong(song);
			await mediumRepository.StoreMedia(new_song, song.Media);
			return new_song;
		}

		public async Task<Song> UpdateSong(Song song)
		{
			var updated_song = await songRepository.UpdateSong(song);
			await mediumRepository.StoreMedia(updated_song, song.Media);
			await songRepository.SaveChangesAsync();
			return updated_song;
		}
		public async Task UpdateTimeline(Song song)
		{
			await songRepository.UpdateTimeline(song);
			await songRepository.SaveChangesAsync();
		}

		public async Task DeleteSong(int song_id)
		{
			await songRepository.DeleteSong(song_id);
			await songRepository.SaveChangesAsync();
		}
	}
}
