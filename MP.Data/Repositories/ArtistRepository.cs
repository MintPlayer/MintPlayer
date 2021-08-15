using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Helpers;
using MintPlayer.Data.Extensions;
using MintPlayer.Pagination;
using MintPlayer.Data.Mappers;

namespace MintPlayer.Data.Repositories
{
    internal interface IArtistRepository
    {
        Task<PaginationResponse<Artist>> PageArtists(Pagination.PaginationRequest<Artist> request);
        Task<IEnumerable<Artist>> GetArtists(bool include_relations, bool include_invisible_media);
        Task<Artist> GetArtist(int id, bool include_relations, bool include_invisible_media);
        Task<Pagination.PaginationResponse<Artist>> PageLikedArtists(Pagination.PaginationRequest<Artist> request);
        Task<IEnumerable<Artist>> GetLikedArtists();
        Task<Artist> InsertArtist(Artist artist);
        Task<Artist> UpdateArtist(Artist artist);
        Task DeleteArtist(int artist_id);
        Task SaveChangesAsync();
    }
    internal class ArtistRepository : IArtistRepository
    {
        private readonly IHttpContextAccessor http_context;
        private readonly MintPlayerContext mintplayer_context;
        private readonly UserManager<Entities.User> user_manager;
        private readonly ArtistHelper artist_helper;
        private readonly SubjectHelper subject_helper;
        private readonly IArtistMapper artistMapper;
        private readonly Jobs.IElasticSearchJobRepository elasticSearchJobRepository;
        public ArtistRepository(
            IHttpContextAccessor http_context,
            MintPlayerContext mintplayer_context,
            UserManager<Entities.User> user_manager,
            ArtistHelper artist_helper,
            SubjectHelper subject_helper,
            IArtistMapper artistMapper,
            Jobs.IElasticSearchJobRepository elasticSearchJobRepository)
        {
            this.http_context = http_context;
            this.mintplayer_context = mintplayer_context;
            this.user_manager = user_manager;
            this.artist_helper = artist_helper;
            this.subject_helper = subject_helper;
            this.artistMapper = artistMapper;
            this.elasticSearchJobRepository = elasticSearchJobRepository;
        }

        public async Task<PaginationResponse<Artist>> PageArtists(Pagination.PaginationRequest<Artist> request)
        {
            var artists = mintplayer_context.Artists;

            // 1) Sort
            var ordered_artists = request.SortDirection == System.ComponentModel.ListSortDirection.Descending
                ? artists.OrderByDescending(request.SortProperty)
                : artists.OrderBy(request.SortProperty);

            // 2) Page
            var paged_artists = ordered_artists
                .Skip((request.Page - 1) * request.PerPage)
                .Take(request.PerPage);

            // 3) Convert to DTO
            var dto_artists = await paged_artists.Select(artist => artistMapper.Entity2Dto(artist, false, false)).ToListAsync();

            var count_artists = await mintplayer_context.Artists.CountAsync();
            return new Pagination.PaginationResponse<Artist>(request, count_artists, dto_artists);
        }

        public Task<IEnumerable<Artist>> GetArtists(bool include_relations, bool include_invisible_media)
        {
            if (include_relations)
            {
                var artists = mintplayer_context.Artists
                    .Include(artist => artist.Members)
                        .ThenInclude(ap => ap.Person)
                    .Include(artist => artist.Songs)
                        .ThenInclude(@as => @as.Song)
                    .Include(artist => artist.Media)
                        .ThenInclude(m => m.Type)
                    .Include(artist => artist.Tags)
                        .ThenInclude(st => st.Tag)
                    .Select(artist => artistMapper.Entity2Dto(artist, include_relations, include_invisible_media));
                return Task.FromResult<IEnumerable<Artist>>(artists);
            }
            else
            {
                var artists = mintplayer_context.Artists
                    .Select(artist => artistMapper.Entity2Dto(artist, include_relations, include_invisible_media));
                return Task.FromResult<IEnumerable<Artist>>(artists);
            }
        }

        public async Task<Artist> GetArtist(int id, bool include_relations, bool include_invisible_media)
        {
            if (include_relations)
            {
                var artist = await mintplayer_context.Artists
                    .Include(a => a.Members)
                        .ThenInclude(ap => ap.Person)
                    .Include(a => a.Songs)
                        .ThenInclude(@as => @as.Song)
                    .Include(a => a.Media)
                        .ThenInclude(m => m.Type)
                    .Include(artist => artist.Tags)
                        .ThenInclude(st => st.Tag)
                            .ThenInclude(t => t.Category)
                    .SingleOrDefaultAsync(a => a.Id == id);
                return artistMapper.Entity2Dto(artist, include_relations, include_invisible_media);
            }
            else
            {
                var artist = await mintplayer_context.Artists
                    .SingleOrDefaultAsync(a => a.Id == id);
                return artistMapper.Entity2Dto(artist, include_relations, include_invisible_media);
            }
        }

        public async Task<Pagination.PaginationResponse<Artist>> PageLikedArtists(Pagination.PaginationRequest<Artist> request)
        {
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            if (user == null) throw new UnauthorizedAccessException();

            var artists = mintplayer_context.Artists;

            // 1) Filter
            var filtered_artists = artists
                .Where(s => s.Likes.Any(l => l.User == user && l.DoesLike));

            // 2) Sort
            var ordered_artists = request.SortDirection == System.ComponentModel.ListSortDirection.Descending
                ? filtered_artists.OrderByDescending(request.SortProperty)
                : filtered_artists.OrderBy(request.SortProperty);

            // 3) Page
            var paged_artists = ordered_artists
                .Skip((request.Page - 1) * request.PerPage)
                .Take(request.PerPage);

            // 4) Convert to DTO
            var dto_artists = paged_artists.Select(artist => artistMapper.Entity2Dto(artist, false, false));

            var count_artists = await filtered_artists.CountAsync();
            return new Pagination.PaginationResponse<Artist>(request, count_artists, dto_artists);
        }

        public async Task<IEnumerable<Artist>> GetLikedArtists()
        {
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            if (user == null) throw new UnauthorizedAccessException();

            var artists = mintplayer_context.Artists
                .Where(s => s.Likes.Any(l => l.User == user && l.DoesLike))
                .Select(s => artistMapper.Entity2Dto(s, false, false));

            return artists;
        }

        public async Task<Artist> InsertArtist(Artist artist)
        {
            // Convert to entity
            var entity_artist = artistMapper.Dto2Entity(artist, mintplayer_context);

            // Get current user
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            entity_artist.UserInsert = user;
            entity_artist.DateInsert = DateTime.Now;

            // Add to database
            await mintplayer_context.Artists.AddAsync(entity_artist);
            await mintplayer_context.SaveChangesAsync();

            var new_artist = artistMapper.Entity2Dto(entity_artist, false, false);
            var job = await elasticSearchJobRepository.InsertElasticSearchIndexJob(new Data.Dtos.Jobs.ElasticSearchIndexJob
            {
                Subject = new_artist,
                SubjectStatus = MintPlayer.Dtos.Enums.eSubjectAction.Added,
                JobStatus = Enums.eJobStatus.Queued
            });

            return new_artist;
        }

        public async Task<Artist> UpdateArtist(Artist artist)
        {
            // Find existing artist
            var artist_entity = await mintplayer_context.Artists
                .Include(a => a.Members)
                    .ThenInclude(ap => ap.Person)
                .Include(a => a.Tags)
                .SingleOrDefaultAsync(a => a.Id == artist.Id);

            if (Convert.ToBase64String(artist_entity.ConcurrencyStamp) != artist.ConcurrencyStamp)
            {
                throw new Exceptions.ConcurrencyException();
            }

            // Set new properties
            artist_entity.Name = artist.Name;
            artist_entity.YearStarted = artist.YearStarted;
            artist_entity.YearQuit = artist.YearQuit;

            IEnumerable<Entities.ArtistPerson> members_to_add, members_to_remove, members_to_update;
            artist_helper.CalculateUpdatedMembers(artist_entity, artist, mintplayer_context, out members_to_add, out members_to_update, out members_to_remove);
            foreach (var item in members_to_remove)
                mintplayer_context.Remove(item);
            foreach (var item in members_to_add)
                await mintplayer_context.AddAsync(item);
            foreach (var item in members_to_update)
                mintplayer_context.Update(item);

            IEnumerable<Entities.SubjectTag> tags_to_add, tags_to_remove;
            subject_helper.CalculateUpdatedTags(artist_entity, artist, mintplayer_context, out tags_to_add, out tags_to_remove);
            foreach (var item in tags_to_remove)
                mintplayer_context.Remove(item);
            foreach (var item in tags_to_add)
                await mintplayer_context.AddAsync(item);

            // Get current user
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            artist_entity.UserUpdate = user;
            artist_entity.DateUpdate = DateTime.Now;

            var updated_artist = artistMapper.Entity2Dto(artist_entity, false, false);
            var job = await elasticSearchJobRepository.InsertElasticSearchIndexJob(new Data.Dtos.Jobs.ElasticSearchIndexJob
            {
                Subject = updated_artist,
                SubjectStatus = MintPlayer.Dtos.Enums.eSubjectAction.Added,
                JobStatus = Data.Enums.eJobStatus.Queued
            });

            return updated_artist;
        }

        public async Task DeleteArtist(int artist_id)
        {
            // Find existing artist
            var artist = mintplayer_context.Artists.Find(artist_id);

            // Get current user
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            artist.UserDelete = user;
            artist.DateDelete = DateTime.Now;

            var deleted_artist = artistMapper.Entity2Dto(artist, false, false);
            var job = await elasticSearchJobRepository.InsertElasticSearchIndexJob(new Data.Dtos.Jobs.ElasticSearchIndexJob
            {
                Subject = deleted_artist,
                SubjectStatus = MintPlayer.Dtos.Enums.eSubjectAction.Deleted,
                JobStatus = Data.Enums.eJobStatus.Queued
            });
        }

        public async Task SaveChangesAsync()
        {
            await mintplayer_context.SaveChangesAsync();
        }
    }
}
