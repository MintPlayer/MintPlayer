using MintPlayer.Dtos.Dtos;

namespace MintPlayer.Data.Abstractions.Services;

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
