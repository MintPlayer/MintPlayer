﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MintPlayer.Data.Mappers
{
    internal interface IPlaylistMapper
    {
        MintPlayer.Dtos.Dtos.Playlist Entity2Dto(Entities.Playlist playlist, bool include_relations = false);
        Entities.Playlist Dto2Entity(MintPlayer.Dtos.Dtos.Playlist playlist, MintPlayerContext mintplayer_context);
    }

    internal class PlaylistMapper : IPlaylistMapper
    {
        private readonly IServiceProvider serviceProvider;
        public PlaylistMapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public MintPlayer.Dtos.Dtos.Playlist Entity2Dto(Entities.Playlist playlist, bool include_relations = false)
        {
            if (playlist == null) return null;
            if (include_relations)
            {
                var userMapper = serviceProvider.GetRequiredService<IUserMapper>();
                var songMapper = serviceProvider.GetRequiredService<ISongMapper>();
                return new MintPlayer.Dtos.Dtos.Playlist
                {
                    Id = playlist.Id,
                    Description = playlist.Description,
                    User = userMapper.Entity2Dto(playlist.User, false),
                    Accessibility = playlist.Accessibility,
                    Tracks = playlist.Tracks
                        .OrderBy(t => t.Index)
                        .Select(t => songMapper.Entity2Dto(t.Song, false, false))
                        .ToList()
                };
            }
            else
            {
                return new MintPlayer.Dtos.Dtos.Playlist
                {
                    Id = playlist.Id,
                    Description = playlist.Description
                };
            }
        }

        public Entities.Playlist Dto2Entity(MintPlayer.Dtos.Dtos.Playlist playlist, MintPlayerContext mintplayer_context)
        {
            if (playlist == null) return null;
            var entity_playlist = new Entities.Playlist
            {
                Id = playlist.Id,
                Description = playlist.Description,
                Accessibility = playlist.Accessibility,
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
    }
}
