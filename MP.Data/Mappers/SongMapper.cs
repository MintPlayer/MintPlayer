using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MintPlayer.Data.Mappers
{
    internal interface ISongMapper
    {
        MintPlayer.Dtos.Dtos.Song Entity2Dto(Entities.Song song, bool include_relations, bool include_invisible_media);
        Entities.Song Dto2Entity(MintPlayer.Dtos.Dtos.Song song, Entities.User user, MintPlayerContext mintplayer_context);
    }

    internal class SongMapper : ISongMapper
    {
        private readonly IServiceProvider serviceProvider;
        public SongMapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public MintPlayer.Dtos.Dtos.Song Entity2Dto(Entities.Song song, bool include_relations, bool include_invisible_media)
        {
            if (song == null) return null;
            if (include_relations)
            {
                var artistMapper = serviceProvider.GetRequiredService<IArtistMapper>();
                var mediumMapper = serviceProvider.GetRequiredService<IMediumMapper>();
                var tagMapper = serviceProvider.GetRequiredService<ITagMapper>();
                var songHelper = serviceProvider.GetRequiredService<SongHelper>();

                var lastLyric = song.Lyrics == null ? null : song.Lyrics.OrderBy(l => l.UpdatedAt).LastOrDefault();
                var playerInfos = songHelper.GetPlayerInfos(song.Media).ToList();

                return new MintPlayer.Dtos.Dtos.Song
                {
                    Id = song.Id,
                    Title = song.Title,
                    Released = song.Released,
                    Lyrics = new MintPlayer.Dtos.Dtos.Lyrics
                    {
                        Text = lastLyric?.Text,
                        Timeline = lastLyric?.Timeline
                    },

                    Text = song.Text,
                    Description = song.Description,
                    YoutubeId = playerInfos.FirstOrDefault(p => p.Type == MintPlayer.Dtos.Enums.ePlayerType.Youtube)?.Id,
                    DailymotionId = playerInfos.FirstOrDefault(p => p.Type == MintPlayer.Dtos.Enums.ePlayerType.DailyMotion)?.Id,
                    VimeoId = playerInfos.FirstOrDefault(p => p.Type == MintPlayer.Dtos.Enums.ePlayerType.Vimeo)?.Id,
                    PlayerInfos = songHelper.GetPlayerInfos(song.Media).ToList(),
                    DateUpdate = song.DateUpdate ?? song.DateInsert,

                    Artists = song.Artists
                        .Where(@as => @as.Credited)
                        .Select(@as => artistMapper.Entity2Dto(@as.Artist, false, false))
                        .ToList(),
                    UncreditedArtists = song.Artists
                        .Where(@as => !@as.Credited)
                        .Select(@as => artistMapper.Entity2Dto(@as.Artist, false, false))
                        .ToList(),
                    Media = song.Media == null ? null : song.Media
                        .Where(m => m.Type.Visible | include_invisible_media)
                        .Select(medium => mediumMapper.Entity2Dto(medium, true))
                        .ToList(),
                    Tags = song.Tags == null ? null : song.Tags
                        .Select(st => tagMapper.Entity2Dto(st.Tag))
                        .ToList()
                };
            }
            else
            {
                var lastLyric = song.Lyrics == null ? null : song.Lyrics.OrderBy(l => l.UpdatedAt).LastOrDefault();
                var songHelper = serviceProvider.GetRequiredService<SongHelper>();
                var playerInfos = songHelper.GetPlayerInfos(song.Media).ToList();
                return new MintPlayer.Dtos.Dtos.Song
                {
                    Id = song.Id,
                    Title = song.Title,
                    Released = song.Released,
                    Lyrics = new MintPlayer.Dtos.Dtos.Lyrics
                    {
                        Text = lastLyric?.Text,
                        Timeline = lastLyric?.Timeline
                    },

                    Text = song.Text,
                    Description = song.Description,
                    YoutubeId = playerInfos.FirstOrDefault(p => p.Type == MintPlayer.Dtos.Enums.ePlayerType.Youtube)?.Id,
                    DailymotionId = playerInfos.FirstOrDefault(p => p.Type == MintPlayer.Dtos.Enums.ePlayerType.DailyMotion)?.Id,
                    VimeoId = playerInfos.FirstOrDefault(p => p.Type == MintPlayer.Dtos.Enums.ePlayerType.Vimeo)?.Id,
                    PlayerInfos = playerInfos,
                    DateUpdate = song.DateUpdate ?? song.DateInsert
                };
            }
        }

        /// <summary>Only use this method for creation of a song</summary>
        public Entities.Song Dto2Entity(MintPlayer.Dtos.Dtos.Song song, Entities.User user, MintPlayerContext mintplayer_context)
        {
            if (song == null) return null;
            var entity_song = new Entities.Song
            {
                Id = song.Id,
                Title = song.Title,
                Released = song.Released
            };
            if (song.Artists != null)
            {
                entity_song.Artists = song.Artists.Select(artist =>
                {
                    var entity_artist = mintplayer_context.Artists.Find(artist.Id);
                    return new Entities.ArtistSong(entity_artist, entity_song);
                }).ToList();
            }
            entity_song.Lyrics = new List<Entities.Lyrics>(new[] {
                new Entities.Lyrics
                {
                    Song = entity_song,
                    User = user,
                    Text = song.Lyrics.Text,
                    Timeline = song.Lyrics.Timeline
                },
            });
            if (song.Media != null)
            {
                var mediumMapper = serviceProvider.GetRequiredService<IMediumMapper>();
                entity_song.Media = song.Media.Select(m =>
                {
                    var medium = mediumMapper.Dto2Entity(m, mintplayer_context);
                    medium.Subject = entity_song;
                    return medium;
                }).ToList();
            }
            #region Tags
            if (song.Tags != null)
            {
                entity_song.Tags = song.Tags.Select(t =>
                {
                    var tag = mintplayer_context.Tags.Find(t.Id);
                    return new Entities.SubjectTag(entity_song, tag);
                }).ToList();
            }
            #endregion

            return entity_song;
        }
    }
}
