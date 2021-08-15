using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Data.Dtos.Blog;
using MintPlayer.Data.Services;

namespace MintPlayer.Web.Server.Controllers.Api.Unversioned
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BlogPostController : Controller
    {
        private readonly IBlogPostService blogPostService;
        public BlogPostController(IBlogPostService blogPostService)
        {
            this.blogPostService = blogPostService;
        }

        /// <summary>Get the blog posts from the database.</summary>
        /// <returns>A list of the blog posts.</returns>
        [HttpGet(Name = "api-blogpost-list")]
        public async Task<ActionResult<IEnumerable<BlogPost>>> Get()
        {
            var blogposts = await blogPostService.GetBlogPosts();
            return Ok(blogposts.ToList());
        }

        /// <summary>Get a specific blog post from the database.</summary>
        /// <param name="id">The id of the blog post to retrieve.</param>
        /// <returns>The blog post with the specified id.</returns>
        [HttpGet("{id}", Name = "api-blogpost-get", Order = 1)]
        public async Task<ActionResult<BlogPost>> Get(int id)
        {
            var blogpost = await blogPostService.GetBlogPost(id);

            if (blogpost == null) return NotFound();
            else return Ok(blogpost);
        }

        /// <summary>Creates a new blog post in the database.</summary>
        /// <param name="blogPost">Blog post to be inserted in the database.</param>
        /// <returns>The newly created blog post with the newly assigned id.</returns>
        [HttpPost(Name = "api-blogpost-create")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Blogger")]
        public async Task<ActionResult<BlogPost>> Post([FromBody] BlogPost blogPost)
        {
            var newBlogPost = await blogPostService.InsertBlogPost(blogPost);
            return Ok(newBlogPost);
        }

        /// <summary>Updates a blog post in the database.</summary>
        /// <param name="blogPost">New blog post information.</param>
        /// <returns>The updated blog post.</returns>
        [HttpPut("{id}", Name = "api-blogpost-update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Blogger")]
        public async Task<ActionResult<BlogPost>> Put([FromBody] BlogPost blogPost)
        {
            var updatedBlogPost = await blogPostService.UpdateBlogPost(blogPost);
            return Ok(updatedBlogPost);
        }

        /// <summary>Deletes a blog post from the database.</summary>
        /// <param name="id">Id of the blog post to delete.</param>
        [HttpDelete("{id}", Name = "api-blogpost-delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Blogger")]
        public async Task<ActionResult> Delete(int id)
        {
            await blogPostService.DeleteBlogPost(id);
            return Ok();
        }
    }
}