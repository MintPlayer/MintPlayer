using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MintPlayer.Data.Mappers
{
    internal interface IPersonMapper
    {
		MintPlayer.Dtos.Dtos.Person Entity2Dto(Entities.Person person, bool include_invisible_media, bool include_relations = false);
		Entities.Person Dto2Entity(MintPlayer.Dtos.Dtos.Person person, MintPlayerContext mintplayer_context);
	}

	internal class PersonMapper : IPersonMapper
	{
        private readonly IServiceProvider serviceProvider;
        public PersonMapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

		public MintPlayer.Dtos.Dtos.Person Entity2Dto(Entities.Person person, bool include_invisible_media, bool include_relations = false)
		{
			if (person == null)
            {
                return null;
            }

			var result = new MintPlayer.Dtos.Dtos.Person
			{
				Id = person.Id,
				FirstName = person.FirstName,
				LastName = person.LastName,
				Born = person.Born,
				Died = person.Died,

				Text = person.Text,
				DateUpdate = person.DateUpdate ?? person.DateInsert,
				ConcurrencyStamp = Convert.ToBase64String(person.ConcurrencyStamp),
			};

			if (include_relations)
			{
				if (person.Artists != null)
				{
					var artistMapper = serviceProvider.GetRequiredService<IArtistMapper>();
					result.Artists = person.Artists
						.Select(ap => artistMapper.Entity2Dto(ap.Artist, include_invisible_media))
						.ToList();
				}

				if (person.Media != null)
				{
					var mediumMapper = serviceProvider.GetRequiredService<IMediumMapper>();
					result.Media = person.Media
						.Where(medium => medium.Type.Visible | include_invisible_media)
						.Select(medium => mediumMapper.Entity2Dto(medium, true))
						.ToList();
				}

				if (person.Tags != null)
				{
					var tagMapper = serviceProvider.GetRequiredService<ITagMapper>();
					result.Tags = person.Tags
						.Select(st => tagMapper.Entity2Dto(st.Tag))
						.ToList();
				}
			}

			return result;
		}

		public Entities.Person Dto2Entity(MintPlayer.Dtos.Dtos.Person person, MintPlayerContext mintplayer_context)
		{
			if (person == null)
            {
                return null;
            }

            var entity_person = new Entities.Person
			{
				Id = person.Id,
				FirstName = person.FirstName,
				LastName = person.LastName,
				Born = person.Born,
				Died = person.Died
			};
			#region Media
			if (person.Media != null)
			{
				var mediumMapper = serviceProvider.GetRequiredService<IMediumMapper>();
				entity_person.Media = person.Media.Select(m =>
				{
					var medium = mediumMapper.Dto2Entity(m, mintplayer_context);
					medium.Subject = entity_person;
					return medium;
				}).ToList();
			}
			#endregion
			#region Tags
			if (person.Tags != null)
			{
				entity_person.Tags = person.Tags.Select(t =>
				{
					var tag = mintplayer_context.Tags.Find(t.Id);
					return new Entities.SubjectTag(entity_person, tag);
				}).ToList();
			}
			#endregion
			return entity_person;
		}
	}
}
