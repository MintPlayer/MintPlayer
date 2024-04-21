namespace MintPlayer.Data.Mappers;

internal interface IArtistMapper
{
	MintPlayer.Dtos.Dtos.Artist? Entity2Dto(Entities.Artist? artist, bool include_invisible_media, bool include_relations = false);
	Entities.Artist? Dto2Entity(MintPlayer.Dtos.Dtos.Artist? artist, MintPlayerContext mintplayer_context);
}


internal class ArtistMapper : IArtistMapper
{
	private readonly IServiceProvider serviceProvider;
	public ArtistMapper(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
	}

	public MintPlayer.Dtos.Dtos.Artist? Entity2Dto(Entities.Artist? artist, bool include_invisible_media, bool include_relations = false)
	{
		if (artist == null)
		{
			return null;
		}

		var result = new MintPlayer.Dtos.Dtos.Artist
		{
			Id = artist.Id,
			Name = artist.Name,
			YearStarted = artist.YearStarted,
			YearQuit = artist.YearQuit,

			Text = artist.Text,
			DateUpdate = artist.DateUpdate ?? artist.DateInsert,
			ConcurrencyStamp = Convert.ToBase64String(artist.ConcurrencyStamp),
		};

		if (include_relations)
		{
			if (artist.Members != null)
			{
				var personMapper = serviceProvider.GetRequiredService<IPersonMapper>();
				result.PastMembers = artist.Members
					.Where(ap => !ap.Active)
					.Select(ap => personMapper.Entity2Dto(ap.Person, include_invisible_media))
					.ToList();
				result.CurrentMembers = artist.Members
					.Where(ap => ap.Active)
					.Select(ap => personMapper.Entity2Dto(ap.Person, include_invisible_media))
					.ToList();
			}

			if (artist.Songs != null)
			{
				var songMapper = serviceProvider.GetRequiredService<ISongMapper>();
				result.Songs = artist.Songs
					.Select(@as => songMapper.Entity2Dto(@as.Song, include_invisible_media))
					.ToList();
			}

			if (artist.Media != null)
			{
				var mediumMapper = serviceProvider.GetRequiredService<IMediumMapper>();
				result.Media = artist.Media == null ? null : artist.Media
					.Where(medium => medium.Type.Visible | include_invisible_media)
					.Select(medium => mediumMapper.Entity2Dto(medium, true))
					.ToList();
			}

			if (artist.Tags != null)
			{
				var tagMapper = serviceProvider.GetRequiredService<ITagMapper>();
				result.Tags = artist.Tags == null ? null : artist.Tags
					.Select(st => tagMapper.Entity2Dto(st.Tag))
					.ToList();
			}
		}

		return result;
	}

	public Entities.Artist? Dto2Entity(MintPlayer.Dtos.Dtos.Artist? artist, MintPlayerContext mintplayer_context)
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
