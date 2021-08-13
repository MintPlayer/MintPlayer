using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Data.Dtos.Blog;
using MintPlayer.Data.Services;

namespace MintPlayer.Web.Server.Controllers.Web.V3
{
    [Controller]
    [Route("web/v3/[controller]")]
    [Authorize(Roles = "Administrator,Blogger")]
    public class BlogPostController : Controller
    {
        private readonly IBlogPostService blogPostService;
        public BlogPostController(IBlogPostService blogPostService)
        {
            this.blogPostService = blogPostService;
        }

        [AllowAnonymous]
        [HttpGet(Name = "web-v3-blogpost-list")]
        public async Task<ActionResult<IEnumerable<BlogPost>>> Get()
        {
            var blogposts = await blogPostService.GetBlogPosts();
            return Ok(blogposts.ToList());
        }

        [AllowAnonymous]
        [HttpGet("{id}", Name = "web-v3-blogpost-get", Order = 1)]
        public async Task<ActionResult<BlogPost>> Get(int id)
        {
            var blogpost = await blogPostService.GetBlogPost(id);

            if (blogpost == null) return NotFound();
            else return Ok(blogpost);
        }

		[ValidateAntiForgeryToken]
        [HttpPost(Name = "web-v3-blogpost-create")]
        public async Task<ActionResult<BlogPost>> Post([FromBody] BlogPost blogPost)
        {
            var newBlogPost = await blogPostService.InsertBlogPost(blogPost);
            return Ok(newBlogPost);
        }

		[ValidateAntiForgeryToken]
        [HttpPut("{id}", Name = "web-v3-blogpost-update")]
        public async Task<ActionResult<BlogPost>> Put([FromBody] BlogPost blogPost)
        {
            var updatedBlogPost = await blogPostService.UpdateBlogPost(blogPost);
            return Ok(updatedBlogPost);
        }

		[ValidateAntiForgeryToken]
        [HttpDelete("{id}", Name = "web-v3-blogpost-delete")]
        public async Task<ActionResult> Delete(int id)
        {
            await blogPostService.DeleteBlogPost(id);
            return Ok();
        }
    }
}