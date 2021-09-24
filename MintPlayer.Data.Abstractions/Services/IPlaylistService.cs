using System.Collections.Generic;
using System.Threading.Tasks;
using MintPlayer.Data.Abstractions.Enums;
using MintPlayer.Dtos.Dtos;

namespace MintPlayer.Data.Abstractions.Services
{
	public interface IPlaylistService
	{
		Task<Pagination.PaginationResponse<Playlist>> PagePlaylists(Pagination.PaginationRequest<Playlist> request, EPlaylistScope playlistScope);
		Task<IEnumerable<Playlist>> GetPlaylists(EPlaylistScope playlistScope, bool include_relations = false);
		Task<Playlist> GetPlaylist(int id, bool include_relations = false);
		Task<Playlist> InsertPlaylist(Playlist playlist);
		Task<Playlist> UpdatePlaylist(Playlist playlist);
		Task DeletePlaylist(int playlistId);
		Task SaveChangesAsync();
	}
}
