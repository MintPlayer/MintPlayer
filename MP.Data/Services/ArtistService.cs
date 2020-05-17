using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MintPlayer.Data.Services
{
    public interface IArtistService
    {
        Task<Pagination.PaginationResponse<Artist>> PageArtists(Pagination.PaginationRequest<Artist> request);
        Task<IEnumerable<Artist>> GetArtists(bool include_relations, bool include_invisible_media);
        Task<Artist> GetArtist(int id, bool include_relations, bool include_invisible_media);
        Task<Pagination.PaginationResponse<Artist>> PageLikedArtists(Pagination.PaginationRequest<Artist> request);
        Task<IEnumerable<Artist>> GetLikedArtists();
        Task<Artist> InsertArtist(Artist artist);
        Task<Artist> UpdateArtist(Artist artist);
        Task DeleteArtist(int artist_id);
        Task SaveChangesAsync();
    }

    internal class ArtistService : IArtistService
    {
        private readonly IArtistRepository artistRepository;
        private readonly IMediumRepository mediumRepository;
        public ArtistService(IArtistRepository artistRepository, IMediumRepository mediumRepository)
        {
            this.artistRepository = artistRepository;
            this.mediumRepository = mediumRepository;
        }

        public async Task<Pagination.PaginationResponse<Artist>> PageArtists(Pagination.PaginationRequest<Artist> request)
        {
            var artists = await artistRepository.PageArtists(request);
            return artists;
        }

        public async Task<IEnumerable<Artist>> GetArtists(bool include_relations, bool include_invisible_media)
        {
            var artists = await artistRepository.GetArtists(include_relations, include_invisible_media);
            return artists;
        }

        public async Task<Artist> GetArtist(int id, bool include_relations, bool include_invisible_media)
        {
            var artist = await artistRepository.GetArtist(id, include_relations, include_invisible_media);
            return artist;
        }

        public Task<Pagination.PaginationResponse<Artist>> PageLikedArtists(Pagination.PaginationRequest<Artist> request)
        {
            return artistRepository.PageLikedArtists(request);
        }

        public Task<IEnumerable<Artist>> GetLikedArtists()
        {
            return artistRepository.GetLikedArtists();
        }

        public async Task<Artist> InsertArtist(Artist artist)
        {
            var new_artist = await artistRepository.InsertArtist(artist);
            await mediumRepository.StoreMedia(new_artist, artist.Media);
            return new_artist;
        }

        public async Task<Artist> UpdateArtist(Artist artist)
        {
            var updated_artist = await artistRepository.UpdateArtist(artist);
            await mediumRepository.StoreMedia(updated_artist, artist.Media);
            await artistRepository.SaveChangesAsync();

            return updated_artist;
        }

        public async Task DeleteArtist(int artist_id)
        {
            await artistRepository.DeleteArtist(artist_id);
            await artistRepository.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await artistRepository.SaveChangesAsync();
        }
    }
}