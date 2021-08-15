using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace MintPlayer.Data.Mappers
{
    internal interface IArtistMapper
    {
        MintPlayer.Dtos.Dtos.Artist Entity2Dto(Entities.Artist artist, bool include_relations, bool include_invisible_media);
        Entities.Artist Dto2Entity(MintPlayer.Dtos.Dtos.Artist artist, MintPlayerContext mintplayer_context);
    }


    internal class ArtistMapper : IArtistMapper
    {
        private readonly IServiceProvider serviceProvider;
        public ArtistMapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public MintPlayer.Dtos.Dtos.Artist Entity2Dto(Entities.Artist artist, bool include_relations, bool include_invisible_media)
        {
            if (artist == null) return null;
            if (include_relations)
            {
                var mediumMapper = serviceProvider.GetRequiredService<IMediumMapper>();
                var personMapper = serviceProvider.GetRequiredService<IPersonMapper>();
                var songMapper = serviceProvider.GetRequiredService<ISongMapper>();
                var tagMapper = serviceProvider.GetRequiredService<ITagMapper>();
                return new MintPlayer.Dtos.Dtos.Artist
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    YearStarted = artist.YearStarted,
                    YearQuit = artist.YearQuit,

                    Text = artist.Text,
                    DateUpdate = artist.DateUpdate ?? artist.DateInsert,
                    ConcurrencyStamp = Convert.ToBase64String(artist.ConcurrencyStamp),

                    PastMembers = artist.Members
                        .Where(ap => !ap.Active)
                        .Select(ap => personMapper.Entity2Dto(ap.Person, false, include_invisible_media))
                        .ToList(),
                    CurrentMembers = artist.Members
                        .Where(ap => ap.Active)
                        .Select(ap => personMapper.Entity2Dto(ap.Person, false, include_invisible_media))
                        .ToList(),
                    Songs = artist.Songs
                        .Select(@as => songMapper.Entity2Dto(@as.Song, false, include_invisible_media))
                        .ToList(),
                    Media = artist.Media == null ? null : artist.Media
                        .Where(medium => medium.Type.Visible | include_invisible_media)
                        .Select(medium => mediumMapper.Entity2Dto(medium, true))
                        .ToList(),
                    Tags = artist.Tags == null ? null : artist.Tags.Select(st => tagMapper.Entity2Dto(st.Tag)).ToList()
                };
            }
            else
            {
                return new MintPlayer.Dtos.Dtos.Artist
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    YearStarted = artist.YearStarted,
                    YearQuit = artist.YearQuit,

                    Text = artist.Text,
                    DateUpdate = artist.DateUpdate ?? artist.DateInsert,
                    ConcurrencyStamp = Convert.ToBase64String(artist.ConcurrencyStamp),
                };
            }
        }

        public Entities.Artist Dto2Entity(MintPlayer.Dtos.Dtos.Artist artist, MintPlayerContext mintplayer_context)
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
            var current = artist.CurrentMembers == null
                ? new Entities.ArtistPerson[0]
                : artist.CurrentMembers.Select(person =>
                {
                    var entity_person = mintplayer_context.People.Find(person.Id);
                    return new Entities.ArtistPerson(entity_artist, entity_person) { Active = true };
                });
            var past = artist.PastMembers == null
                ? new Entities.ArtistPerson[0]
                : artist.PastMembers.Select(person =>
                {
                    var entity_person = mintplayer_context.People.Find(person.Id);
                    return new Entities.ArtistPerson(entity_artist, entity_person) { Active = false };
                });

            entity_artist.Members = current.Union(past).ToList();
            #endregion
            #region Media
            if (artist.Media != null)
            {
                var mediumMapper = serviceProvider.GetRequiredService<IMediumMapper>();
                entity_artist.Media = artist.Media.Select(m =>
                {
                    var medium = mediumMapper.Dto2Entity(m, mintplayer_context);
                    medium.Subject = entity_artist;
                    return medium;
                }).ToList();
            }
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
    }
}
