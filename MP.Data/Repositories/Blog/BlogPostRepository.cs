using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MintPlayer.Data.Dtos.Blog;
using MintPlayer.Data.Entities;
using MintPlayer.Data.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MintPlayer.Data.Repositories.Blog
{
    internal interface IBlogPostRepository
    {
        Task<IEnumerable<BlogPost>> GetBlogPosts();
        Task<BlogPost> GetBlogPost(int blogpost_id);
        Task<BlogPost> InsertBlogPost(BlogPost blogPost);
        Task<BlogPost> UpdateBlogPost(BlogPost blogPost);
        Task DeleteBlogPost(int blogpost_id);
        Task SaveChangesAsync();
    }

    internal class BlogPostRepository : IBlogPostRepository
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly MintPlayerContext mintPlayerContext;
        private readonly UserManager<Entities.User> userManager;
        private readonly IBlogPostMapper blogPostMapper;
        public BlogPostRepository(
            IHttpContextAccessor httpContextAccessor,
            MintPlayerContext mintPlayerContext,
            UserManager<Entities.User> userManager,
            IBlogPostMapper blogPostMapper)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.mintPlayerContext = mintPlayerContext;
            this.userManager = userManager;
            this.blogPostMapper = blogPostMapper;
        }

        public Task<IEnumerable<BlogPost>> GetBlogPosts()
        {
            var blogposts = mintPlayerContext.BlogPosts
                .Include(bp => bp.UserInsert)
                .OrderByDescending(bp => bp.DateInsert)
                .Select(bp => blogPostMapper.Entity2Dto(bp));
            return Task.FromResult<IEnumerable<BlogPost>>(blogposts);
        }
        public async Task<BlogPost> GetBlogPost(int blogpost_id)
        {
            var blogpost = await mintPlayerContext.BlogPosts
                .Include(bp => bp.UserInsert)
                .SingleOrDefaultAsync(bp => bp.Id == blogpost_id);
            var dto = blogPostMapper.Entity2Dto(blogpost);
            return dto;
        }
        public async Task<BlogPost> InsertBlogPost(BlogPost blogPost)
        {
            // Convert to entity
            var entityBlogPost = blogPostMapper.Dto2Entity(blogPost, mintPlayerContext);

            // Get current user
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
            var isBlogger = await userManager.IsInRoleAsync(user, "Blogger");
            if (!isBlogger) throw new UnauthorizedAccessException();

            // Keep track of user
            entityBlogPost.UserInsert = user;
            entityBlogPost.DateInsert = DateTime.Now;

            // Add to database
            await mintPlayerContext.BlogPosts.AddAsync(entityBlogPost);
            await mintPlayerContext.SaveChangesAsync();

            return blogPostMapper.Entity2Dto(entityBlogPost);
        }
        public async Task<BlogPost> UpdateBlogPost(BlogPost blogPost)
        {
            // Find existing blogpost
            var entityBlogPost = await mintPlayerContext.BlogPosts.FindAsync(blogPost.Id);

            // Set new properties
            entityBlogPost.Title = blogPost.Title;
            entityBlogPost.Headline = blogPost.Headline;
            entityBlogPost.Body = blogPost.Body;

            // Get current user
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
            var isBlogger = await userManager.IsInRoleAsync(user, "Blogger");
            if (!isBlogger) throw new UnauthorizedAccessException();

            // Keep track of user
            entityBlogPost.UserUpdate = user;
            entityBlogPost.DateUpdate = DateTime.Now;

            // Update in database
            mintPlayerContext.Entry(entityBlogPost).State = EntityState.Modified;

            return blogPostMapper.Entity2Dto(entityBlogPost);
        }
        public async Task DeleteBlogPost(int blogpost_id)
        {
            // Find existing blogpost
            var blogPost = await mintPlayerContext.BlogPosts.FindAsync(blogpost_id);

            // Get current user
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
            var isBlogger = await userManager.IsInRoleAsync(user, "Blogger");
            if (!isBlogger) throw new UnauthorizedAccessException();

            // Keep track of user
            blogPost.UserDelete = user;
            blogPost.DateDelete = DateTime.Now;
        }
        public async Task SaveChangesAsync()
        {
            await mintPlayerContext.SaveChangesAsync();
        }
    }
}
