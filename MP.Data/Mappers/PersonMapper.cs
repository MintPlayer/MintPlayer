using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MintPlayer.Data.Mappers
{
    internal interface IPersonMapper
    {
		MintPlayer.Dtos.Dtos.Person Entity2Dto(Entities.Person person, bool include_relations, bool include_invisible_media);
		Entities.Person Dto2Entity(MintPlayer.Dtos.Dtos.Person person, MintPlayerContext mintplayer_context);
	}

	internal class PersonMapper : IPersonMapper
	{
        private readonly IServiceProvider serviceProvider;
        public PersonMapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

		public MintPlayer.Dtos.Dtos.Person Entity2Dto(Entities.Person person, bool include_relations, bool include_invisible_media)
		{
			if (person == null) return null;
			if (include_relations)
			{
				var artistMapper = serviceProvider.GetRequiredService<IArtistMapper>();
				var mediumMapper = serviceProvider.GetRequiredService<IMediumMapper>();
				var tagMapper = serviceProvider.GetRequiredService<ITagMapper>();

				return new MintPlayer.Dtos.Dtos.Person
				{
					Id = person.Id,
					FirstName = person.FirstName,
					LastName = person.LastName,
					Born = person.Born,
					Died = person.Died,

					Text = person.Text,
					DateUpdate = person.DateUpdate ?? person.DateInsert,

					Artists = person.Artists
						.Select(ap => artistMapper.Entity2Dto(ap.Artist, false, include_invisible_media))
						.ToList(),
					Media = person.Media == null ? null : person.Media
						.Where(medium => medium.Type.Visible | include_invisible_media)
						.Select(medium => mediumMapper.Entity2Dto(medium, true))
						.ToList(),
					Tags = person.Tags == null ? null : person.Tags
						.Select(st => tagMapper.Entity2Dto(st.Tag))
						.ToList()
				};
			}
			else
			{
				return new MintPlayer.Dtos.Dtos.Person
				{
					Id = person.Id,
					FirstName = person.FirstName,
					LastName = person.LastName,
					Born = person.Born,
					Died = person.Died,

					Text = person.Text,
					DateUpdate = person.DateUpdate ?? person.DateInsert
				};
			}
		}

		public Entities.Person Dto2Entity(MintPlayer.Dtos.Dtos.Person person, MintPlayerContext mintplayer_context)
		{
			if (person == null) return null;
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
