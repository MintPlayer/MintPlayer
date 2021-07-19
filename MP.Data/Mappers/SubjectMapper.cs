using Microsoft.Extensions.DependencyInjection;
using System;

namespace MintPlayer.Data.Mappers
{
    internal interface ISubjectMapper
    {
        MintPlayer.Dtos.Dtos.Subject Entity2Dto(Entities.Subject subject, bool include_relations, bool include_invisible_media);
    }

    internal class SubjectMapper : ISubjectMapper
    {
        private readonly IServiceProvider serviceProvider;
        public SubjectMapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public MintPlayer.Dtos.Dtos.Subject Entity2Dto(Entities.Subject subject, bool include_relations, bool include_invisible_media)
        {
            if (subject == null) return null;

            var subject_type = subject.GetType();
            if (subject_type == typeof(Entities.Person))
            {
                var personMapper = serviceProvider.GetRequiredService<IPersonMapper>();
                return personMapper.Entity2Dto((Entities.Person)subject, include_relations, include_invisible_media);
            }
            else if (subject_type == typeof(Entities.Artist))
            {
                var artistMapper = serviceProvider.GetRequiredService<IArtistMapper>();
                return artistMapper.Entity2Dto((Entities.Artist)subject, include_relations, include_invisible_media);
            }
            else if (subject_type == typeof(Entities.Song))
            {
                var songMapper = serviceProvider.GetRequiredService<ISongMapper>();
                return songMapper.Entity2Dto((Entities.Song)subject, include_relations, include_invisible_media);
            }
            else
            {
                throw new ArgumentException("The subject type was not recognized", nameof(subject));
            }
        }
    }
}
