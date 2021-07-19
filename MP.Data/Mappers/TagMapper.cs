using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MintPlayer.Data.Mappers
{
    internal interface ITagMapper
    {
        MintPlayer.Dtos.Dtos.Tag Entity2Dto(Entities.Tag tag, bool include_subjects = false);
        Entities.Tag Dto2Entity(MintPlayer.Dtos.Dtos.Tag tag, MintPlayerContext mintplayer_context);
    }

    internal class TagMapper : ITagMapper
    {
        private readonly IServiceProvider serviceProvider;
        public TagMapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public MintPlayer.Dtos.Dtos.Tag Entity2Dto(Entities.Tag tag, bool include_subjects = false)
        {
            if (tag == null) return null;

            if (include_subjects)
            {
                var tagCategoryMapper = serviceProvider.GetRequiredService<ITagCategoryMapper>();
                var subjectMapper = serviceProvider.GetRequiredService<ISubjectMapper>();
                return new MintPlayer.Dtos.Dtos.Tag
                {
                    Id = tag.Id,
                    Description = tag.Description,
                    Subjects = tag.Subjects.Select(s => subjectMapper.Entity2Dto(s.Subject, false, false)).ToList(),
                    Category = tagCategoryMapper.Entity2Dto(tag.Category),

                    Parent = Entity2Dto(tag.Parent),
                    Children = tag.Children.Select(t => Entity2Dto(t)).ToList()
                };
            }
            else
            {
                var tagCategoryMapper = serviceProvider.GetRequiredService<ITagCategoryMapper>();
                return new MintPlayer.Dtos.Dtos.Tag
                {
                    Id = tag.Id,
                    Description = tag.Description,
                    Category = tagCategoryMapper.Entity2Dto(tag.Category)
                };
            }
        }

        public Entities.Tag Dto2Entity(MintPlayer.Dtos.Dtos.Tag tag, MintPlayerContext mintplayer_context)
        {
            if (tag == null) return null;
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
}
