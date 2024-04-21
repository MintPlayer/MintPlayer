namespace MintPlayer.Data.Mappers;

internal interface ISubjectMapper
{
	MintPlayer.Dtos.Dtos.Subject? Entity2Dto(Entities.Subject? subject, bool include_invisible_media, bool include_relations = false);
}

internal class SubjectMapper : ISubjectMapper
{
	private readonly IServiceProvider serviceProvider;
	public SubjectMapper(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
	}

	public MintPlayer.Dtos.Dtos.Subject? Entity2Dto(Entities.Subject? subject, bool include_invisible_media, bool include_relations = false)
	{
		if (subject == null) return null;

		switch (subject)
		{
			case Entities.Person person:
				var personMapper = serviceProvider.GetRequiredService<IPersonMapper>();
				return personMapper.Entity2Dto(person, include_invisible_media, include_relations);
			case Entities.Artist artist:
				var artistMapper = serviceProvider.GetRequiredService<IArtistMapper>();
				return artistMapper.Entity2Dto(artist, include_invisible_media, include_relations);
			case Entities.Song song:
				var songMapper = serviceProvider.GetRequiredService<ISongMapper>();
				return songMapper.Entity2Dto(song, include_invisible_media, include_relations);
			default:
				throw new ArgumentException("The subject type was not recognized", nameof(subject));
		}
	}
}
