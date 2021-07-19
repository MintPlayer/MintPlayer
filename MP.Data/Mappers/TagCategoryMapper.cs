using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MintPlayer.Data.Mappers
{
    internal interface ITagCategoryMapper
    {
        MintPlayer.Dtos.Dtos.TagCategory Entity2Dto(Entities.TagCategory tagCategory, bool include_relations = false);
        Entities.TagCategory Dto2Entity(MintPlayer.Dtos.Dtos.TagCategory tagCategory, MintPlayerContext mintplayer_context);
    }

    internal class TagCategoryMapper : ITagCategoryMapper
    {
        private readonly IServiceProvider serviceProvider;
        public TagCategoryMapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public MintPlayer.Dtos.Dtos.TagCategory Entity2Dto(Entities.TagCategory tagCategory, bool include_relations = false)
        {
            if (tagCategory == null) return null;
            if (include_relations)
            {
                var tagMapper = serviceProvider.GetRequiredService<ITagMapper>();
                return new MintPlayer.Dtos.Dtos.TagCategory
                {
                    Id = tagCategory.Id,
                    Color = tagCategory.Color,
                    Description = tagCategory.Description,
                    Tags = tagCategory.Tags == null
                        ? new List<MintPlayer.Dtos.Dtos.Tag>()
                        : tagCategory.Tags.Where(t => t.Parent == null).Select(t => tagMapper.Entity2Dto(t)).ToList(),
                    TotalTagCount = tagCategory.Tags == null
                        ? 0
                        : tagCategory.Tags.Count()
                };
            }
            else
            {
                return new MintPlayer.Dtos.Dtos.TagCategory
                {
                    Id = tagCategory.Id,
                    Color = tagCategory.Color,
                    Description = tagCategory.Description
                };
            }
        }

        public Entities.TagCategory Dto2Entity(MintPlayer.Dtos.Dtos.TagCategory tagCategory, MintPlayerContext mintplayer_context)
        {
            if (tagCategory == null) return null;

            var entity_tag_category = new Entities.TagCategory
            {
                Id = tagCategory.Id,
                Color = tagCategory.Color,
                Description = tagCategory.Description
            };
            return entity_tag_category;
        }
    }
}
