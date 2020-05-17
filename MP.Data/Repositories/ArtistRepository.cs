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

namespace MintPlayer.Data.Repositories
{
    internal interface IArtistRepository
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
    internal class ArtistRepository : IArtistRepository
    {
        private readonly IHttpContextAccessor http_context;
        private readonly MintPlayerContext mintplayer_context;
        private readonly UserManager<Entities.User> user_manager;
        private readonly ArtistHelper artist_helper;
        private readonly SubjectHelper subject_helper;
        private readonly Jobs.IElasticSearchJobRepository elasticSearchJobRepository;
        public ArtistRepository(IHttpContextAccessor http_context, MintPlayerContext mintplayer_context, UserManager<Entities.User> user_manager, ArtistHelper artist_helper, SubjectHelper subject_helper, Jobs.IElasticSearchJobRepository elasticSearchJobRepository)
        {
            this.http_context = http_context;
            this.mintplayer_context = mintplayer_context;
            this.user_manager = user_manager;
            this.artist_helper = artist_helper;
            this.subject_helper = subject_helper;
			this.elasticSearchJobRepository = elasticSearchJobRepository;
        }

        public async Task<Pagination.PaginationResponse<Artist>> PageArtists(Pagination.PaginationRequest<Artist> request)
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
            var dto_artists = paged_artists.Select(artist => ToDto(artist, false, false));

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
                    .Select(artist => ToDto(artist, include_relations, include_invisible_media));
                return Task.FromResult<IEnumerable<Artist>>(artists);
            }
            else
            {
                var artists = mintplayer_context.Artists
                    .Select(artist => ToDto(artist, include_relations, include_invisible_media));
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
                return ToDto(artist, include_relations, include_invisible_media);
            }
            else
            {
                var artist = await mintplayer_context.Artists
                    .SingleOrDefaultAsync(a => a.Id == id);
                return ToDto(artist, include_relations, include_invisible_media);
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
            var dto_artists = paged_artists.Select(artist => ToDto(artist, false, false));

            var count_artists = await filtered_artists.CountAsync();
            return new Pagination.PaginationResponse<Artist>(request, count_artists, dto_artists);
        }

        public async Task<IEnumerable<Artist>> GetLikedArtists()
        {
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            if (user == null) throw new UnauthorizedAccessException();

            var artists = mintplayer_context.Artists
                .Where(s => s.Likes.Any(l => l.User == user && l.DoesLike))
                .Select(s => ToDto(s, false, false));

            return artists;
        }

        public async Task<Artist> InsertArtist(Artist artist)
        {
            // Convert to entity
            var entity_artist = ToEntity(artist, mintplayer_context);

            // Get current user
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            entity_artist.UserInsert = user;
            entity_artist.DateInsert = DateTime.Now;

            // Add to database
            await mintplayer_context.Artists.AddAsync(entity_artist);
            await mintplayer_context.SaveChangesAsync();

            var new_artist = ToDto(entity_artist, false, false);
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

            var updated_artist = ToDto(artist_entity, false, false);
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

            var deleted_artist = ToDto(artist, false, false);
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

        #region Conversion methods
        internal static Artist ToDto(Entities.Artist artist, bool include_relations, bool include_invisible_media)
        {
            if (artist == null) return null;
            if (include_relations)
            {
                return new Artist
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    YearStarted = artist.YearStarted,
                    YearQuit = artist.YearQuit,

                    Text = artist.Text,
                    DateUpdate = artist.DateUpdate ?? artist.DateInsert,

                    PastMembers = artist.Members
                        .Where(ap => !ap.Active)
                        .Select(ap => PersonRepository.ToDto(ap.Person, false, include_invisible_media))
                        .ToList(),
                    CurrentMembers = artist.Members
                        .Where(ap => ap.Active)
                        .Select(ap => PersonRepository.ToDto(ap.Person, false, include_invisible_media))
                        .ToList(),
                    Songs = artist.Songs
                        .Select(@as => SongRepository.ToDto(@as.Song, false, include_invisible_media))
                        .ToList(),
                    Media = artist.Media == null ? null : artist.Media
                        .Where(medium => medium.Type.Visible | include_invisible_media)
                        .Select(medium => MediumRepository.ToDto(medium, true))
                        .ToList(),
                    Tags = artist.Tags == null ? null : artist.Tags.Select(st => TagRepository.ToDto(st.Tag)).ToList()
                };
            }
            else
            {
                return new Artist
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    YearStarted = artist.YearStarted,
                    YearQuit = artist.YearQuit,

                    Text = artist.Text,
                    DateUpdate = artist.DateUpdate ?? artist.DateInsert
                };
            }
        }
        internal static Entities.Artist ToEntity(Artist artist, MintPlayerContext mintplayer_context)
        {
            if (artist == null) return null;
            var entity_artist = new Entities.Artist
            {
                Id = artist.Id,
                Name = artist.Name,
                YearStarted = artist.YearStarted,
                YearQuit = artist.YearQuit
            };

            #region Members
            var artist_person = artist.CurrentMembers.Select(person => {
                var entity_person = mintplayer_context.People.Find(person.Id);
                return new Entities.ArtistPerson(entity_artist, entity_person) { Active = true };
            }).Union(artist.PastMembers.Select(person => {
                var entity_person = mintplayer_context.People.Find(person.Id);
                return new Entities.ArtistPerson(entity_artist, entity_person) { Active = false };
            }));
            entity_artist.Members = artist_person.ToList();
            #endregion
            #region Media
            entity_artist.Media = artist.Media.Select(m => {
                var medium = MediumRepository.ToEntity(m, mintplayer_context);
                medium.Subject = entity_artist;
                return medium;
            }).ToList();
            #endregion
            #region Tags
            if (artist.Tags != null)
            {
                entity_artist.Tags = artist.Tags.Select(t =>
                {
                    var tag = mintplayer_context.Tags.Find(t.Id);
                    return new Entities.SubjectTag(entity_artist, tag);
                }).ToList();
            }
            #endregion

            return entity_artist;
        }
        #endregion
    }
}
