using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using MintPlayer.Pagination;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Data.Enums;

namespace MintPlayer.Data.Services
{
    public interface IPlaylistService
    {
        Task<Pagination.PaginationResponse<Playlist>> PagePlaylists(Pagination.PaginationRequest<Playlist> request, ePlaylistScope playlistScope);
        Task<IEnumerable<Playlist>> GetPlaylists(ePlaylistScope playlistScope, bool include_relations = false);
        Task<Playlist> GetPlaylist(int id, bool include_relations = false);
        Task<Playlist> InsertPlaylist(Playlist playlist);
        Task<Playlist> UpdatePlaylist(Playlist playlist);
        Task DeletePlaylist(int playlistId);
        Task SaveChangesAsync();
    }
    internal class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository playlistRepository;
        public PlaylistService(IPlaylistRepository playlistRepository)
        {
            this.playlistRepository = playlistRepository;
        }

        public async Task<PaginationResponse<Playlist>> PagePlaylists(PaginationRequest<Playlist> request, ePlaylistScope playlistScope)
        {
            var playlists = await playlistRepository.PagePlaylists(request, playlistScope);
            return playlists;
        }

        public async Task<IEnumerable<Playlist>> GetPlaylists(ePlaylistScope playlistScope, bool include_relations = false)
        {
            var playlists = await playlistRepository.GetPlaylists(playlistScope, include_relations);
            return playlists;
        }

        public async Task<Playlist> GetPlaylist(int id, bool include_relations = false)
        {
            var playlist = await playlistRepository.GetPlaylist(id, include_relations);
            return playlist;
        }

        public async Task<Playlist> InsertPlaylist(Playlist playlist)
        {
            var new_playlist = await playlistRepository.InsertPlaylist(playlist);
            return new_playlist;
        }

        public async Task<Playlist> UpdatePlaylist(Playlist playlist)
        {
            var updated_playlist = await playlistRepository.UpdatePlaylist(playlist);
            return updated_playlist;
        }

        public async Task DeletePlaylist(int playlistId)
        {
            await playlistRepository.DeletePlaylist(playlistId);
            await playlistRepository.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await playlistRepository.SaveChangesAsync();
        }
    }
}
