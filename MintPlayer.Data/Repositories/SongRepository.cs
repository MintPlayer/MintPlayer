﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Data.Extensions;
using MintPlayer.Data.Mappers;

namespace MintPlayer.Data.Repositories
{
	internal interface ISongRepository
	{
		Task<Pagination.PaginationResponse<Song>> PageSongs(Pagination.PaginationRequest<Song> request);
		Task<IEnumerable<Song>> GetSongs(bool include_relations, bool include_invisible_media);
		Task<Song> GetSong(int id, bool include_relations, bool include_invisible_media);
		Task<Pagination.PaginationResponse<Song>> PageLikedSongs(Pagination.PaginationRequest<Song> request);
		Task<IEnumerable<Song>> GetLikedSongs();
		Task<Song> InsertSong(Song song);
		Task<Song> UpdateSong(Song song);
		Task UpdateTimeline(Song song);
		Task DeleteSong(int song_id);
		Task SaveChangesAsync();
	}

	internal class SongRepository : ISongRepository
	{
		private readonly IHttpContextAccessor http_context;
		private readonly MintPlayerContext mintplayer_context;
		private readonly UserManager<Entities.User> user_manager;
		private readonly SongHelper song_helper;
		private readonly SubjectHelper subject_helper;
        private readonly ISongMapper songMapper;
        private readonly Jobs.IElasticSearchJobRepository elasticSearchJobRepository;
		public SongRepository(
			IHttpContextAccessor http_context,
			MintPlayerContext mintplayer_context,
			UserManager<Entities.User> user_manager,
			SongHelper song_helper,
			SubjectHelper subject_helper,
			ISongMapper songMapper,
			Jobs.IElasticSearchJobRepository elasticSearchJobRepository)
		{
			this.http_context = http_context;
			this.mintplayer_context = mintplayer_context;
			this.user_manager = user_manager;
			this.song_helper = song_helper;
            this.subject_helper = subject_helper;
            this.songMapper = songMapper;
            this.elasticSearchJobRepository = elasticSearchJobRepository;
		}

        public async Task<Pagination.PaginationResponse<Song>> PageSongs(Pagination.PaginationRequest<Song> request)
        {
            var songs = mintplayer_context.Songs;

            // 1) Sort
            var ordered_songs = request.SortDirection == System.ComponentModel.ListSortDirection.Descending
                ? songs.OrderByDescending(request.SortProperty)
                : songs.OrderBy(request.SortProperty);

            // 2) Page
            var paged_songs = ordered_songs
			   .Skip((request.Page - 1) * request.PerPage)
			   .Take(request.PerPage);

            // 3) Convert to DTO
            var dto_songs = await paged_songs.Select(song => songMapper.Entity2Dto(song, false, false)).ToListAsync();

            var count_songs = await mintplayer_context.Songs.CountAsync();
            return new Pagination.PaginationResponse<Song>(request, count_songs, dto_songs);

            //var songs = mintplayer_context.Songs
            //	.Select(song => ToDto(song, false, false))
            //	.ToList();

            //return new Pagination.PaginationResponse<Song>(
            //	request,
            //	songs.Count,
            //	songs
            //);
        }

		public Task<IEnumerable<Song>> GetSongs(bool include_relations, bool include_invisible_media)
		{
			if (include_relations)
			{
				var songs = mintplayer_context.Songs
					.Include(song => song.Lyrics)
					.Include(song => song.Artists)
						.ThenInclude(@as => @as.Artist)
					.Include(song => song.Media)
						.ThenInclude(m => m.Type)
					.Include(song => song.Tags)
						.ThenInclude(st => st.Tag)
					.Select(song => songMapper.Entity2Dto(song, include_invisible_media, include_relations));
				return Task.FromResult<IEnumerable<Song>>(songs);
			}
			else
			{
				var songs = mintplayer_context.Songs
					//.Include(song => song.Lyrics)
					.Include(song => song.Artists)
						.ThenInclude(@as => @as.Artist)
					.Include(song => song.Media)
						.ThenInclude(m => m.Type)
					.Select(song => songMapper.Entity2Dto(song, include_invisible_media, include_relations));
				return Task.FromResult<IEnumerable<Song>>(songs);
			}
		}

		public async Task<Song> GetSong(int id, bool include_relations, bool include_invisible_media)
		{
			if (include_relations)
			{
				var song = await mintplayer_context.Songs
					.Include(s => s.Lyrics)
					.Include(s => s.Artists)
						.ThenInclude(@as => @as.Artist)
					.Include(s => s.Media)
						.ThenInclude(m => m.Type)
					.Include(s => s.Tags)
						.ThenInclude(st => st.Tag)
							.ThenInclude(t => t.Category)
					.SingleOrDefaultAsync(s => s.Id == id);
				return songMapper.Entity2Dto(song, include_invisible_media, include_relations);
			}
			else
			{
				var song = await mintplayer_context.Songs
					.Include(s => s.Artists)
						.ThenInclude(@as => @as.Artist)
					.Include(s => s.Media)
						.ThenInclude(m => m.Type)
					.SingleOrDefaultAsync(s => s.Id == id);
				return songMapper.Entity2Dto(song, include_invisible_media, include_relations);
			}
		}

		public async Task<Pagination.PaginationResponse<Song>> PageLikedSongs(Pagination.PaginationRequest<Song> request)
		{
			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			if (user == null) throw new UnauthorizedAccessException();

			var songs = mintplayer_context.Songs;

			// 1) Filter
			var filtered_songs = songs
				.Where(s => s.Likes.Any(l => l.User == user && l.DoesLike));

			// 2) Sort
			var ordered_songs = request.SortDirection == System.ComponentModel.ListSortDirection.Descending
				? filtered_songs.OrderByDescending(request.SortProperty)
				: filtered_songs.OrderBy(request.SortProperty);

			// 3) Page
			var paged_songs = ordered_songs
				.Skip((request.Page - 1) * request.PerPage)
				.Take(request.PerPage);

			// 4) Convert to DTO
			var dto_songs = paged_songs.Select(song => songMapper.Entity2Dto(song, false, false));

			var count_songs = await filtered_songs.CountAsync();
			return new Pagination.PaginationResponse<Song>(request, count_songs, dto_songs);
		}

		public async Task<IEnumerable<Song>> GetLikedSongs()
		{
			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			if (user == null) throw new UnauthorizedAccessException();

			var songs = mintplayer_context.Songs
				.Where(s => s.Likes.Any(l => l.User == user && l.DoesLike))
				.Select(s => songMapper.Entity2Dto(s, false, false));

			return songs;
		}

		public async Task<Song> InsertSong(Song song)
		{
			// Get current user
			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);

			// Convert to entity
			var entity_song = songMapper.Dto2Entity(song, user, mintplayer_context);
			entity_song.UserInsert = user;
			entity_song.DateInsert = DateTime.Now;

			// Add to database
			await mintplayer_context.Songs.AddAsync(entity_song);
			await mintplayer_context.SaveChangesAsync();

			var new_song = songMapper.Entity2Dto(entity_song, false, false);
			var job = await elasticSearchJobRepository.InsertElasticSearchIndexJob(new Abstractions.Dtos.Jobs.ElasticSearchIndexJob
			{
				Subject = new_song,
				SubjectStatus = MintPlayer.Dtos.Enums.eSubjectAction.Added,
				JobStatus = Abstractions.Enums.EJobStatus.Queued
			});

			return new_song;
		}

		public async Task<Song> UpdateSong(Song song)
		{
			// Find existing song
			var song_entity = await mintplayer_context.Songs
				.Include(s => s.Artists)
					.ThenInclude(@as => @as.Artist)
				.Include(s => s.Lyrics)
				.Include(s => s.Tags)
				.SingleOrDefaultAsync(s => s.Id == song.Id);

			if (song_entity == null)
			{
				throw new Exceptions.NotFoundException();
			}

			if (Convert.ToBase64String(song_entity.ConcurrencyStamp) != song.ConcurrencyStamp)
			{
				var databaseValue = songMapper.Entity2Dto(song_entity, false, true);
				throw Exceptions.ConcurrencyException.Create(databaseValue);
			}

			// Set new properties
			song_entity.Title = song.Title;
			song_entity.Released = song.Released;

			// Add/update/delete artists
			IEnumerable<Entities.ArtistSong> to_add, to_remove, to_update;
			song_helper.CalculateUpdatedArtists(song_entity, song, mintplayer_context, out to_add, out to_update, out to_remove);
			foreach (var item in to_remove)
				mintplayer_context.Entry(item).State = EntityState.Deleted;
			foreach (var item in to_add)
				mintplayer_context.Entry(item).State = EntityState.Added;
			foreach (var item in to_update)
				mintplayer_context.Entry(item).State = EntityState.Modified;

			IEnumerable<Entities.SubjectTag> tags_to_add, tags_to_remove;
			subject_helper.CalculateUpdatedTags(song_entity, song, mintplayer_context, out tags_to_add, out tags_to_remove);
			foreach (var item in tags_to_remove)
				mintplayer_context.Remove(item);
			foreach (var item in tags_to_add)
				await mintplayer_context.AddAsync(item);

			// Set UserUpdate
			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			song_entity.UserUpdate = user;
			song_entity.DateUpdate = DateTime.Now;

			// Add/update lyrics
			var lyrics = song_entity.Lyrics.FirstOrDefault(l => l.UserId == user.Id);
			if (lyrics == null)
			{
				lyrics = new Entities.Lyrics(user, song_entity);
				lyrics.Text = song.Lyrics.Text;
				// Don't update the timeline
				//lyrics.Timeline = song.Lyrics.Timeline;
				lyrics.UpdatedAt = DateTime.Now;
				mintplayer_context.Entry(lyrics).State = EntityState.Added;
			}
			else
			{
				lyrics.Text = song.Lyrics.Text;
				// Don't update the timeline
				//lyrics.Timeline = song.Lyrics.Timeline;
				lyrics.UpdatedAt = DateTime.Now;
				mintplayer_context.Entry(lyrics).State = EntityState.Modified;
			}

			// Update
			mintplayer_context.Entry(song_entity).State = EntityState.Modified;

			var updated_song = songMapper.Entity2Dto(song_entity, false, false);
			var job = await elasticSearchJobRepository.InsertElasticSearchIndexJob(new Abstractions.Dtos.Jobs.ElasticSearchIndexJob
			{
				Subject = updated_song,
				SubjectStatus = MintPlayer.Dtos.Enums.eSubjectAction.Added,
				JobStatus = Abstractions.Enums.EJobStatus.Queued
			});

			return updated_song;
		}
		 
		public async Task UpdateTimeline(Song song)
		{
			var song_entity = await mintplayer_context.Songs
				.Include(s => s.Lyrics)
				.SingleOrDefaultAsync(s => s.Id == song.Id);

			// Set UserUpdate
			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			song_entity.UserUpdate = user;
			song_entity.DateUpdate = DateTime.Now;

			// Set timeline
			var lyrics = song_entity.Lyrics.FirstOrDefault(l => l.UserId == user.Id);
			if(lyrics == null)
			{
				throw new Exception("No lyrics available for this song");
			}
			else
			{
				lyrics.Timeline = song.Lyrics.Timeline;
				mintplayer_context.Entry(lyrics).State = EntityState.Modified;
			}

			// Update
			mintplayer_context.Entry(song_entity).State = EntityState.Modified;
		}

		public async Task DeleteSong(int song_id)
		{
			// Find existing song
			var song = await mintplayer_context.Songs.FindAsync(song_id);

			if (song == null)
			{
				throw new Exceptions.NotFoundException();
			}

			// Get current user
			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			song.UserDelete = user;
			song.DateDelete = DateTime.Now;

			var deleted_song = songMapper.Entity2Dto(song, false, false);
			var job = await elasticSearchJobRepository.InsertElasticSearchIndexJob(new Abstractions.Dtos.Jobs.ElasticSearchIndexJob
			{
				Subject = deleted_song,
				SubjectStatus = MintPlayer.Dtos.Enums.eSubjectAction.Deleted,
				JobStatus = Abstractions.Enums.EJobStatus.Queued
			});
		}

		public async Task SaveChangesAsync()
		{
			await mintplayer_context.SaveChangesAsync();
		}
	}
}
