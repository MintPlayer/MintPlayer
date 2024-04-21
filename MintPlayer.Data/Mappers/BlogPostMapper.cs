namespace MintPlayer.Data.Mappers;

internal interface IBlogPostMapper
{
	Abstractions.Dtos.Blog.BlogPost? Entity2Dto(Entities.Blog.BlogPost? blogPost);
	Entities.Blog.BlogPost? Dto2Entity(Abstractions.Dtos.Blog.BlogPost? blogPost, MintPlayerContext mintPlayerContext);
}

internal class BlogPostMapper : IBlogPostMapper
{
	private readonly IServiceProvider serviceProvider;
	public BlogPostMapper(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
	}

	public Abstractions.Dtos.Blog.BlogPost? Entity2Dto(Entities.Blog.BlogPost? blogPost)
	{
		if (blogPost == null)
		{
			return null;
		}

		var userMapper = serviceProvider.GetRequiredService<IUserMapper>();

		var result = new Abstractions.Dtos.Blog.BlogPost
		{
			Id = blogPost.Id,
			Title = blogPost.Title,
			Headline = blogPost.Headline,
			Body = blogPost.Body,
			Published = blogPost.DateInsert,
		};

		if (blogPost.UserInsert != null)
		{
			result.Author = userMapper.Entity2Dto(blogPost.UserInsert, false);
		}

		return result;
	}
	public Entities.Blog.BlogPost? Dto2Entity(Abstractions.Dtos.Blog.BlogPost? blogPost, MintPlayerContext mintPlayerContext)
	{
		if (blogPost == null)
		{
			return null;
		}

		return new Entities.Blog.BlogPost
		{
			Id = blogPost.Id,
			Title = blogPost.Title,
			Headline = blogPost.Headline,
			Body = blogPost.Body
		};
	}
}
