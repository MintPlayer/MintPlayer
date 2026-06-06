using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Dtos.Dtos;

namespace MintPlayer.Data.Abstractions.Services
{
	public interface IArtistService
	{
		Task<Pagination.PaginationResponse<Artist>> PageArtists(Pagination.PaginationRequest<Artist> request);
		Task<IEnumerable<Artist>> GetArtists(bool include_relations);
		Task<Artist> GetArtist(int id, bool include_relations);
		Task<Pagination.PaginationResponse<Artist>> PageLikedArtists(Pagination.PaginationRequest<Artist> request);
		Task<IEnumerable<Artist>> GetLikedArtists();
		Task<Artist> InsertArtist(Artist artist);
		Task<Artist> UpdateArtist(Artist artist);
		Task DeleteArtist(int artist_id);
		Task SaveChangesAsync();
	}
}
