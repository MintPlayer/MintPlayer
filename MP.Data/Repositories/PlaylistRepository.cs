using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MintPlayer.Dtos.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using MintPlayer.Data.Helpers;

namespace MintPlayer.Data.Repositories
{
    internal interface IPlaylistRepository
    {
        Task<Pagination.PaginationResponse<Playlist>> PagePlaylists(Pagination.PaginationRequest<Playlist> request);
        Task<IEnumerable<Playlist>> GetPlaylists(bool include_relations = false);
        Task<Playlist> GetPlaylist(int id, bool include_relations = false);
        Task<Playlist> InsertPlaylist(Playlist playlist);
        Task<Playlist> UpdatePlaylist(Playlist playlist);
        Task DeletePlaylist(int playlistId);
        Task SaveChangesAsync();
    }

    internal class PlaylistRepository : IPlaylistRepository
    {
        private readonly IHttpContextAccessor http_context;
        private readonly MintPlayerContext mintplayer_context;
        private readonly UserManager<Entities.User> user_manager;
        private readonly TrackHelper trackHelper;
        public PlaylistRepository(IHttpContextAccessor http_context, MintPlayerContext mintplayer_context, UserManager<Entities.User> user_manager, TrackHelper trackHelper)
        {
            this.http_context = http_context;
            this.mintplayer_context = mintplayer_context;
            this.user_manager = user_manager;
            this.trackHelper = trackHelper;
        }

        public async Task<Pagination.PaginationResponse<Playlist>> PagePlaylists(Pagination.PaginationRequest<Playlist> request)
        {
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);

            var playlists = mintplayer_context.Playlists
                .Where(p => p.User == user);

            // 1) Sort
            var ordered_playlists = request.SortDirection == System.ComponentModel.ListSortDirection.Descending
                ? playlists.OrderByDescending(request.SortProperty)
                : playlists.OrderBy(request.SortProperty);

            // 2) Page
            var paged_playlists = ordered_playlists
                .Skip((request.Page - 1) * request.PerPage)
                .Take(request.PerPage);

            // 3) Convert to DTO
            var dto_playlists = await paged_playlists.Select(p => ToDto(p, false)).ToListAsync();

            var count_playlists = await playlists.CountAsync();
            return new Pagination.PaginationResponse<Playlist>(request, count_playlists, dto_playlists);


            //var playlists = await mintplayer_context.Playlists
            //    //.Where(p => p.User == user)
            //    .Select(p => ToDto(p, false))
            //    .ToListAsync();
            //var count = await mintplayer_context.Playlists.CountAsync();
            //return new Pagination.PaginationResponse<Playlist>(request, count, playlists);
        }

        public async Task<IEnumerable<Playlist>> GetPlaylists(bool include_relations = false)
        {
            // Get current user
            var current_user = await user_manager.GetUserAsync(http_context.HttpContext.User);

            if (include_relations)
            {
                var playlists = mintplayer_context.Playlists
                    .Include(p => p.Tracks)
                        .ThenInclude(track => track.Song)
                            .ThenInclude(s => s.Media)
                                .ThenInclude(m => m.Type)
                    .Include(p => p.User)
                    .Where(p => p.User == current_user)
                    .Select(p => ToDto(p, true));
                return playlists;
            }
            else
            {
                var playlists = mintplayer_context.Playlists
                    .Where(p => p.User == current_user)
                    .Select(p => ToDto(p, false));
                return playlists;
            }
        }

        public async Task<Playlist> GetPlaylist(int id, bool include_relations = false)
        {
            // Get current user
            var current_user = await user_manager.GetUserAsync(http_context.HttpContext.User);

            if (include_relations)
            {
                var playlist = await mintplayer_context.Playlists
                    .Include(p => p.Tracks)
                        .ThenInclude(t => t.Song)
                            .ThenInclude(s => s.Artists)
                                .ThenInclude(@as => @as.Artist)
                    .Include(p => p.Tracks)
                        .ThenInclude(t => t.Song)
                            .ThenInclude(s => s.Media)
                                .ThenInclude(m => m.Type)
                    .Include(p => p.Tracks)
                        .ThenInclude(t => t.Song)
                            .ThenInclude(s => s.Lyrics)
                    .Include(p => p.User)
                    .Where(p => p.User == current_user)
                    .SingleOrDefaultAsync(p => p.Id == id);
                return ToDto(playlist, true);
            }
            else
            {
                var playlist = await mintplayer_context.Playlists
                    .Where(p => p.User == current_user)
                    .SingleOrDefaultAsync(p => p.Id == id);
                return ToDto(playlist);
            }
        }

        public async Task<Playlist> InsertPlaylist(Playlist playlist)
        {
            // Convert to entity
            var entity_playlist = ToEntity(playlist, mintplayer_context);

            // Get current user
            entity_playlist.User = await user_manager.GetUserAsync(http_context.HttpContext.User);

            // Add to database
            await mintplayer_context.Playlists.AddAsync(entity_playlist);
            await mintplayer_context.SaveChangesAsync();

            var new_playlist = ToDto(entity_playlist);
            return new_playlist;
        }

        public async Task<Playlist> UpdatePlaylist(Playlist playlist)
        {
            //var playlist_entities = await mintplayer_context.Playlists.ToListAsync();
            //throw new NotImplementedException();

            // Get current user
            var current_user = await user_manager.GetUserAsync(http_context.HttpContext.User);

            // Find existing playlist
            var playlist_entity = await mintplayer_context.Playlists
                .Include(p => p.Tracks)
                    .ThenInclude(track => track.Song)
                        .ThenInclude(song => song.Artists)
                            .ThenInclude(@as => @as.Artist)
                .Where(p => p.User == current_user)
                .Where(p => p.Id == playlist.Id)
                .SingleOrDefaultAsync();

            // Set new properties
            playlist_entity.Description = playlist.Description;

            //IEnumerable<Entities.PlaylistSong> tracks_to_add, tracks_to_remove, tracks_to_update;
            //trackHelper.CalculateUpdatedTracks(playlist_entity, playlist, mintplayer_context, out tracks_to_add, out tracks_to_update, out tracks_to_remove);
            //foreach (var item in tracks_to_remove)
            //    mintplayer_context.Remove(item);
            //foreach (var item in tracks_to_add)
            //    await mintplayer_context.AddAsync(item);
            //foreach (var item in tracks_to_update)
            //    mintplayer_context.Update(item);

            //mintplayer_context.RemoveRange(playlist_entity.Tracks);
            playlist_entity.Tracks.Clear();
            playlist_entity.Tracks.AddRange(
                playlist.Tracks
                    .Select((song, index) => new Entities.PlaylistSong
                    {
                        Playlist = playlist_entity,
                        PlaylistId = playlist_entity.Id,
                        Song = mintplayer_context.Songs.Find(song.Id),
                        SongId = song.Id,
                        Index = index
                    })
            );

            await mintplayer_context.SaveChangesAsync();

            return ToDto(playlist_entity, true);
        }

        public async Task DeletePlaylist(int playlistId)
        {
            // Get current user
            var current_user = await user_manager.GetUserAsync(http_context.HttpContext.User);

            // Find existing playlist
            var playlist = await mintplayer_context.Playlists
                .Where(p => p.User == current_user)
                .SingleOrDefaultAsync(p => p.Id == playlistId);

            // Mark as deleted
            playlist.IsDeleted = true;
        }

        public async Task SaveChangesAsync()
        {
            await mintplayer_context.SaveChangesAsync();
        }

        #region Conversion methods
        internal static Playlist ToDto(Entities.Playlist playlist, bool include_relations = false)
        {
            if (playlist == null) return null;
            if (include_relations)
            {
                return new Playlist
                {
                    Id = playlist.Id,
                    Description = playlist.Description,
                    User = AccountRepository.ToDto(playlist.User),
                    Tracks = playlist.Tracks
                        .OrderBy(t => t.Index)
                        .Select(t => SongRepository.ToDto(t.Song, false, false))
                        .ToList()
                };
            }
            else
            {
                return new Playlist
                {
                    Id = playlist.Id,
                    Description = playlist.Description
                };
            }
        }

        internal static Entities.Playlist ToEntity(Playlist playlist, MintPlayerContext mintplayer_context)
        {
            if (playlist == null) return null;
            var entity_playlist = new Entities.Playlist
            {
                Id = playlist.Id,
                Description = playlist.Description
            };

            #region Tracks
            if (playlist.Tracks != null)
            {
                entity_playlist.Tracks = playlist.Tracks.Select((song, index) =>
                {
                    var entity_song = mintplayer_context.Songs.Find(song.Id);
                    return new Entities.PlaylistSong(entity_playlist, entity_song) { Index = index };
                }).ToList();
            }
            #endregion

            return entity_playlist;
        }
        #endregion
    }
}
