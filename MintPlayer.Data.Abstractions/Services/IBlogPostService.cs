using MintPlayer.Data.Abstractions.Dtos.Blog;

namespace MintPlayer.Data.Abstractions.Services;

public interface IBlogPostService
{
	Task<IEnumerable<BlogPost>> GetBlogPosts();
	Task<BlogPost> GetBlogPost(int blogpost_id);
	Task<BlogPost> InsertBlogPost(BlogPost blogPost);
	Task<BlogPost> UpdateBlogPost(BlogPost blogPost);
	Task DeleteBlogPost(int blogpost_id);
}
