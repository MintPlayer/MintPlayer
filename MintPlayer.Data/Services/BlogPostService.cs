﻿using MintPlayer.Data.Abstractions.Dtos.Blog;
using MintPlayer.Data.Abstractions.Services;
using MintPlayer.Data.Repositories.Blog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MintPlayer.Data.Services
{
    internal class BlogPostService : IBlogPostService
    {
        private readonly IBlogPostRepository blogPostRepository;
        public BlogPostService(IBlogPostRepository blogPostRepository)
        {
            this.blogPostRepository = blogPostRepository;
        }

        public Task<IEnumerable<BlogPost>> GetBlogPosts()
        {
            return blogPostRepository.GetBlogPosts();
        }
        public Task<BlogPost> GetBlogPost(int blogpost_id)
        {
            return blogPostRepository.GetBlogPost(blogpost_id);
        }
        public Task<BlogPost> InsertBlogPost(BlogPost blogPost)
        {
            return blogPostRepository.InsertBlogPost(blogPost);
        }
        public async Task<BlogPost> UpdateBlogPost(BlogPost blogPost)
        {
            var updatedBlogPost = await blogPostRepository.UpdateBlogPost(blogPost);
            await blogPostRepository.SaveChangesAsync();
            return updatedBlogPost;
        }
        public async Task DeleteBlogPost(int blogPostId)
        {
            await blogPostRepository.DeleteBlogPost(blogPostId);
            await blogPostRepository.SaveChangesAsync();
        }
    }
}
