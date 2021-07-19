using System;
using Microsoft.Extensions.DependencyInjection;

namespace MintPlayer.Data.Mappers
{
    internal interface IBlogPostMapper
    {
        MintPlayer.Data.Dtos.Blog.BlogPost Entity2Dto(Entities.Blog.BlogPost blogPost);
        Entities.Blog.BlogPost Dto2Entity(MintPlayer.Data.Dtos.Blog.BlogPost blogPost, MintPlayerContext mintPlayerContext);
    }

    internal class BlogPostMapper : IBlogPostMapper
    {
        private readonly IServiceProvider serviceProvider;
        public BlogPostMapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public MintPlayer.Data.Dtos.Blog.BlogPost Entity2Dto(Entities.Blog.BlogPost blogPost)
        {
            if (blogPost == null) return null;

            var userMapper = serviceProvider.GetRequiredService<IUserMapper>();

            return new MintPlayer.Data.Dtos.Blog.BlogPost
            {
                Id = blogPost.Id,
                Title = blogPost.Title,
                Headline = blogPost.Headline,
                Body = blogPost.Body,
                Author = userMapper.Entity2Dto(blogPost.UserInsert, false),
                Published = blogPost.DateInsert
            };
        }
        public Entities.Blog.BlogPost Dto2Entity(MintPlayer.Data.Dtos.Blog.BlogPost blogPost, MintPlayerContext mintPlayerContext)
        {
            if (blogPost == null) return null;
            return new Entities.Blog.BlogPost
            {
                Id = blogPost.Id,
                Title = blogPost.Title,
                Headline = blogPost.Headline,
                Body = blogPost.Body
            };
        }
    }
}
