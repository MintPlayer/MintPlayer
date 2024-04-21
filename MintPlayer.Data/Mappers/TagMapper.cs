namespace MintPlayer.Data.Mappers;

internal interface ITagMapper
{
	MintPlayer.Dtos.Dtos.Tag? Entity2Dto(Entities.Tag? tag, bool include_subjects = false, bool include_tags = false);
	Entities.Tag? Dto2Entity(MintPlayer.Dtos.Dtos.Tag? tag, MintPlayerContext mintplayer_context);
}

internal class TagMapper : ITagMapper
{
	private readonly IServiceProvider serviceProvider;
	public TagMapper(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
	}

	public MintPlayer.Dtos.Dtos.Tag? Entity2Dto(Entities.Tag? tag, bool include_subjects = false, bool include_tags = false)
	{
		if (tag == null)
		{
			return null;
		}

		var result = new MintPlayer.Dtos.Dtos.Tag
		{
			Id = tag.Id,
			Description = tag.Description,
		};

		if (tag.Category != null)
		{
			var tagCategoryMapper = serviceProvider.GetRequiredService<ITagCategoryMapper>();
			result.Category = tagCategoryMapper.Entity2Dto(tag.Category);
		}

		if (include_subjects)
		{
			if (tag.Subjects != null)
			{
				var subjectMapper = serviceProvider.GetRequiredService<ISubjectMapper>();
				result.Subjects = tag.Subjects.Select(s => subjectMapper.Entity2Dto(s.Subject, false)).ToList();
			}
		}

		if (include_tags)
		{
			var tagCategoryMapper = serviceProvider.GetRequiredService<ITagCategoryMapper>();

			result.Parent = Entity2Dto(tag.Parent);

			if (tag.Children != null)
			{
				result.Children = tag.Children.Select(t => Entity2Dto(t)).ToList();
			}
		}

		return result;
	}

	public Entities.Tag? Dto2Entity(MintPlayer.Dtos.Dtos.Tag? tag, MintPlayerContext mintplayer_context)
	{
		if (tag == null)
		{
			return null;
		}

		var entity_tag = new Entities.Tag
		{
			Id = tag.Id,
			Description = tag.Description,
			Category = mintplayer_context.TagCategories.Find(tag.Category.Id)
		};

		if (tag.Parent != null)
		{
			entity_tag.Parent = mintplayer_context.Tags.Find(tag.Parent.Id);
		}

		return entity_tag;
	}
}
