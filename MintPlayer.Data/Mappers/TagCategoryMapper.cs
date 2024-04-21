namespace MintPlayer.Data.Mappers;

internal interface ITagCategoryMapper
{
	MintPlayer.Dtos.Dtos.TagCategory? Entity2Dto(Entities.TagCategory? tagCategory, bool include_relations = false);
	Entities.TagCategory? Dto2Entity(MintPlayer.Dtos.Dtos.TagCategory? tagCategory, MintPlayerContext mintplayer_context);
}

internal class TagCategoryMapper : ITagCategoryMapper
{
	private readonly IServiceProvider serviceProvider;
	public TagCategoryMapper(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
	}

	public MintPlayer.Dtos.Dtos.TagCategory? Entity2Dto(Entities.TagCategory? tagCategory, bool include_relations = false)
	{
		if (tagCategory == null)
		{
			return null;
		}

		var result = new MintPlayer.Dtos.Dtos.TagCategory
		{
			Id = tagCategory.Id,
			Color = tagCategory.Color,
			Description = tagCategory.Description,
		};

		if (include_relations)
		{
			if (tagCategory.Tags != null)
			{
				var tagMapper = serviceProvider.GetRequiredService<ITagMapper>();
				result.Tags = tagCategory.Tags.Where(t => t.Parent == null).Select(t => tagMapper.Entity2Dto(t)).ToList();
				result.TotalTagCount = tagCategory.Tags.Count();
			}
		}

		return result;
	}

	public Entities.TagCategory? Dto2Entity(MintPlayer.Dtos.Dtos.TagCategory? tagCategory, MintPlayerContext mintplayer_context)
	{
		if (tagCategory == null)
		{
			return null;
		}

		var entity_tag_category = new Entities.TagCategory
		{
			Id = tagCategory.Id,
			Color = tagCategory.Color,
			Description = tagCategory.Description
		};

		return entity_tag_category;
	}
}
