using Microsoft.AspNetCore.Mvc;

namespace MintPlayer.Web.Server.Controllers.Web
{
    [Route("robots.txt")]
    public class RobotsTxtController : Controller
    {
		// GET: robots.txt
        [HttpGet(Name = "web-robots")]
        public IActionResult Index()
        {
            Response.ContentType = "text/plain";

            var model = new ViewModels.RobotsTxt.IndexVM
            {
                SitemapUrl = Url.RouteUrl("web-sitemap-index", new { }, Request.Scheme, Request.Host.Value)
            };
            return View(model);
        }
    }
}