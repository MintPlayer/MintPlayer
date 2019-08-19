using Microsoft.AspNetCore.Mvc;

namespace MintPlayer.Web.Server.Controllers
{
    [Route("")]
    public class RobotsController : Controller
    {
        [HttpGet("robots.txt", Name = "robots-index")]
        public IActionResult Index()
        {
            return Content($"Sitemap: {Url.RouteUrl("sitemap-index", new { }, Request.Scheme)}");
        }
    }
}