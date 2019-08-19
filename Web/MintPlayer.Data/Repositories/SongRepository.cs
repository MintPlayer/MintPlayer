using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MintPlayer.Data.Helpers;
using MintPlayer.Data.Repositories.Interfaces;
using MintPlayer.Data.Dtos;

namespace MintPlayer.Data.Repositories
{
    internal class SongRepository : ISongRepository
    {
        private IHttpContextAccessor http_context;
        private MintPlayerContext mintplayer_context;
        private UserManager<Entities.User> user_manager;
        private SongHelper song_helper;
        private Nest.IElasticClient elastic_client;
        public SongRepository(IHttpContextAccessor http_context, MintPlayerContext mintplayer_context, UserManager<Entities.User> user_manager, SongHelper song_helper, Nest.IElasticClient elastic_client)
        {
            this.http_context = http_context;
            this.mintplayer_context = mintplayer_context;
            this.user_manager = user_manager;
            this.song_helper = song_helper;
            this.elastic_client = elastic_client;
        }

        public IEnumerable<Dtos.Song> GetSongs(bool include_relations = false)
        {
            if (include_relations)
            {
                var songs = mintplayer_context.Songs
                    .Include(song => song.Lyrics)
                    .Include(song => song.Artists)
                        .ThenInclude(@as => @as.Artist)
                    .Include(song => song.Media)
                        .ThenInclude(m => m.Type)
                    .Select(song => ToDto(song, true));
                return songs;
            }
            else
            {
                var songs = mintplayer_context.Songs
                    //.Include(song => song.Lyrics)
                    .Select(song => ToDto(song, false));
                return songs;
            }
        }

        public IEnumerable<Song> GetSongs(int count, int page, bool include_relations = false)
        {
            if (include_relations)
            {
                var songs = mintplayer_context.Songs
                    .Include(song => song.Lyrics)
                    .Include(song => song.Artists)
                        .ThenInclude(@as => @as.Artist)
                    .Include(song => song.Media)
                        .ThenInclude(m => m.Type)
                    .Skip((page - 1) * count)
                    .Take(count)
                    .Select(song => ToDto(song, true));
                return songs;
            }
            else
            {
                var songs = mintplayer_context.Songs
                    //.Include(song => song.Lyrics)
                    .Skip((page - 1) * count)
                    .Take(count)
                    .Select(song => ToDto(song, false));
                return songs;
            }
        }

        public Dtos.Song GetSong(int id, bool include_relations = false)
        {
            if (include_relations)
            {
                var song = mintplayer_context.Songs
                    .Include(s => s.Lyrics)
                    .Include(s => s.Artists)
                        .ThenInclude(@as => @as.Artist)
                    .Include(s => s.Media)
                        .ThenInclude(m => m.Type)
                    .SingleOrDefault(s => s.Id == id);
                return ToDto(song, true);
            }
            else
            {
                var song = mintplayer_context.Songs
                    .Include(s => s.Lyrics)
                    .SingleOrDefault(s => s.Id == id);
                return ToDto(song, false);
            }
        }

        public async Task<Dtos.Song> InsertSong(Dtos.Song song)
        {
            // Get current user
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);

            // Convert to entity
            var entity_song = ToEntity(song, user, mintplayer_context);
            entity_song.UserInsert = user;
            entity_song.DateInsert = DateTime.Now;

            // Add to database
            mintplayer_context.Songs.Add(entity_song);
            mintplayer_context.SaveChanges();

            // Index
            var new_song = ToDto(entity_song);
            var index_status = await elastic_client.IndexDocumentAsync(new_song);

            return new_song;
        }

        public async Task<Dtos.Song> UpdateSong(Dtos.Song song)
        {
            // Find existing song
            var song_entity = mintplayer_context.Songs.Include(s => s.Artists)
                .Include(s => s.Lyrics)
                .SingleOrDefault(s => s.Id == song.Id);

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

            // Set UserUpdate
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            song_entity.UserUpdate = user;
            song_entity.DateUpdate = DateTime.Now;

            // Add/update lyrics
            var lyrics = song_entity.Lyrics.FirstOrDefault(l => l.UserId == user.Id);
            if (lyrics == null)
            {
                lyrics = new Entities.Lyrics(user, song_entity);
                lyrics.Text = song.Lyrics;
                lyrics.UpdatedAt = DateTime.Now;
                mintplayer_context.Entry(lyrics).State = EntityState.Added;
            }
            else
            {
                lyrics.Text = song.Lyrics;
                lyrics.UpdatedAt = DateTime.Now;
                mintplayer_context.Entry(lyrics).State = EntityState.Modified;
            }

            // Update
            mintplayer_context.Entry(song_entity).State = EntityState.Modified;

            // Index
            var updated_song = ToDto(song_entity);
            await elastic_client.UpdateAsync<Song>(updated_song, u => u.Doc(updated_song));

            return updated_song;
        }

        public async Task DeleteSong(int song_id)
        {
            // Find existing song
            var song = mintplayer_context.Songs.Find(song_id);

            // Get current user
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            song.UserDelete = user;
            song.DateDelete = DateTime.Now;

            // Index
            var song_dto = ToDto(song);
            await elastic_client.DeleteAsync<Song>(song_dto);
        }

        public async Task SaveChangesAsync()
        {
            await mintplayer_context.SaveChangesAsync();
        }

        #region Conversion methods
        internal static Dtos.Song ToDto(Entities.Song song, bool include_relations = false)
        {
            if (song == null) return null;
            if (include_relations)
            {
                return new Dtos.Song
                {
                    Id = song.Id,
                    Title = song.Title,
                    Released = song.Released,
                    Lyrics = song.Lyrics.OrderBy(l => l.UpdatedAt).LastOrDefault()?.Text,

                    Artists = song.Artists.Select(@as => ArtistRepository.ToDto(@as.Artist)).ToList(),
                    Media = song.Media.Select(medium => MediumRepository.ToDto(medium, true)).ToList(),

                    DateUpdate = song.DateUpdate ?? song.DateInsert
                };
            }
            else
            {
                return new Dtos.Song
                {
                    Id = song.Id,
                    Title = song.Title,
                    Released = song.Released,
                    Lyrics = song.Lyrics?.OrderBy(l => l.UpdatedAt).LastOrDefault()?.Text,

                    DateUpdate = song.DateUpdate ?? song.DateInsert
                };
            }
        }
        /// <summary>Only use this method for creation of a song</summary>
        internal static Entities.Song ToEntity(Dtos.Song song, Entities.User user, MintPlayerContext mintplayer_context)
        {
            if (song == null) return null;
            var entity_song = new Entities.Song
            {
                Id = song.Id,
                Title = song.Title,
                Released = song.Released
            };
            entity_song.Artists = song.Artists.Select(artist =>
            {
                var entity_artist = mintplayer_context.Artists.Find(artist.Id);
                return new Entities.ArtistSong(entity_artist, entity_song);
            }).ToList();
            entity_song.Lyrics = new List<Entities.Lyrics>(new[] {
                new Entities.Lyrics { Song = entity_song, User = user, Text = song.Lyrics }
            });
            entity_song.Media = song.Media.Select(m => {
                var medium = MediumRepository.ToEntity(m, mintplayer_context);
                medium.Subject = entity_song;
                return medium;
            }).ToList();
            return entity_song;
        }
        #endregion
    }
}
