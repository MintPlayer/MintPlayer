using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.AspNetCore.SitemapXml;
using MintPlayer.AspNetCore.SitemapXml.Abstractions;
using MintPlayer.Web.Extensions;
using MintPlayer.Dtos.Dtos;
using MintPlayer.AspNetCore.SpaServices.Routing;
using MintPlayer.Data.Abstractions.Services;
using MintPlayer.AspNetCore.SitemapXml.Abstractions.Data;

namespace MintPlayer.Web.Server.Controllers.Web
{
	[Controller]
    [Route("[controller]")]
    public class SitemapController : Controller
    {
        private readonly ISitemapXml sitemapXml;
        private readonly IPersonService personService;
        private readonly IArtistService artistService;
        private readonly ISongService songService;
        private readonly ISpaRouteService spaRouteService;
        public SitemapController(ISitemapXml sitemapXml, IPersonService personService, IArtistService artistService, ISongService songService, ISpaRouteService spaRouteService)
        {
            this.sitemapXml = sitemapXml;
            this.personService = personService;
            this.artistService = artistService;
            this.songService = songService;
            this.spaRouteService = spaRouteService;
        }

        // GET: web/Sitemap
        [Produces("text/xml")]
        [HttpGet(Name = "web-sitemap-index")]
		[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<SitemapIndex> Index()
        {
            const int per_page = 100;

            var people = await personService.GetPeople(false);
            var artists = await artistService.GetArtists(false);
            var songs = await songService.GetSongs(false);

            var person_urls = sitemapXml.GetSitemapIndex(people, per_page, (perPage, page) => Url.RouteUrl("web-sitemap-sitemap", new { subject = "person", count = perPage, page }, Request.Scheme));
            var artist_urls = sitemapXml.GetSitemapIndex(artists, per_page, (perPage, page) => Url.RouteUrl("web-sitemap-sitemap", new { subject = "artist", count = perPage, page }, Request.Scheme));
            var song_urls = sitemapXml.GetSitemapIndex(songs, per_page, (perPage, page) => Url.RouteUrl("web-sitemap-sitemap", new { subject = "song", count = perPage, page }, Request.Scheme));

            return new SitemapIndex(person_urls.Concat(artist_urls).Concat(song_urls));
        }

        // GET: web/Sitemap/{subject}/{count}/{page}
        [Produces("text/xml")]
        [HttpGet("{subject}/{count}/{page}", Name = "web-sitemap-sitemap")]
		[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Sitemap(string subject, int count, int page)
        {
            string[] languages = new string[] { "fr", "nl" };
            IEnumerable<Subject> subjects;
            switch (subject.ToLower())
            {
                case "person":
                    var people = await personService.GetPeople(false);
                    subjects = people.Skip((page - 1) * count).Take(count);
                    break;
                case "artist":
                    var artists = await artistService.GetArtists(false);
                    subjects = artists.Skip((page - 1) * count).Take(count);
                    break;
                case "song":
                    var songs = await songService.GetSongs(false);
                    subjects = songs.Skip((page - 1) * count).Take(count);
                    break;
                default:
                    return NotFound();
			}
			Func<Subject, Task<Url>> subject2url = async (s) =>
			{
				switch (subject)
				{
					case "person":
						{
							Func<string, Task<Link>> func = async (lang) =>
							{
								return new Link
								{
									Rel = "alternate",
									HrefLang = lang,
									Href = await spaRouteService.GenerateUrl($"person-show-name", new { id = s.Id, name = s.Text.Slugify(), lang }, HttpContext),
								};
							};
							var links = await Task.WhenAll(languages.Select(lang => func(lang)));

							return new Url
							{
								Loc = await spaRouteService.GenerateUrl($"person-show-name", new { id = s.Id, name = s.Text.Slugify() }, HttpContext),
								ChangeFreq = AspNetCore.SitemapXml.Abstractions.Enums.ChangeFreq.Monthly,
								LastMod = s.DateUpdate,
								Links = links.ToList(),
							};
						}
					case "artist":
						{
							Func<string, Task<Link>> func = async (lang) =>
							{
								return new Link
								{
									Rel = "alternate",
									HrefLang = lang,
									Href = await spaRouteService.GenerateUrl($"artist-show-name", new { id = s.Id, name = s.Text.Slugify(), lang }, HttpContext),
								};
							};
							var links = await Task.WhenAll(languages.Select(lang => func(lang)));

							return new Url
							{
								Loc = await spaRouteService.GenerateUrl($"artist-show-name", new { id = s.Id, name = s.Text.Slugify() }, HttpContext),
								ChangeFreq = AspNetCore.SitemapXml.Abstractions.Enums.ChangeFreq.Monthly,
								LastMod = s.DateUpdate,
								Links = links.ToList()
							};
						}
					case "song":
						{
							Func<Song, List<Video>> parseSongVideos = (song) =>
							{
								if (string.IsNullOrEmpty(song.YoutubeId))
								{
									return new List<Video>();
								}
								else
								{
									return new List<Video>
									{
										new Video
										{
											Title = song.Title,
											Description = song.Description,
											ContentLocation = $"https://www.youtube.com/watch?v={song.YoutubeId}",
											PlayerLocation = $"https://www.youtube.com/embed/{song.YoutubeId}",
											ThumbnailLocation = $"http://i.ytimg.com/vi/{song.YoutubeId}/hqdefault.jpg"
										}
									};
								}
							};
							Func<Song, List<Image>> parseSongImages = (song) =>
							{
								if (string.IsNullOrEmpty(song.YoutubeId))
								{
									return new List<Image>();
								}
								else
								{
									return new List<Image>
									{
										new Image
										{
											Title = song.Description,
											Caption = song.Title,
											Location = $"http://i.ytimg.com/vi/{song.YoutubeId}/default.jpg"
										}
									};
								}
							};

							Func<string, Task<Link>> func = async (lang) =>
							{
								return new Link
								{
									Rel = "alternate",
									HrefLang = lang,
									Href = await spaRouteService.GenerateUrl($"song-show-title", new { id = s.Id, title = s.Text.Slugify(), lang }, HttpContext),
								};
							};
							var links = await Task.WhenAll(languages.Select(lang => func(lang)));

							return new Url
							{
								Loc = await spaRouteService.GenerateUrl($"song-show-title", new { id = s.Id, title = s.Text.Slugify() }, HttpContext),
								ChangeFreq = AspNetCore.SitemapXml.Abstractions.Enums.ChangeFreq.Monthly,
								LastMod = s.DateUpdate,
								Links = links.ToList(),
								Videos = parseSongVideos((Song)s),
								Images = parseSongImages((Song)s)
							};
						}
					default:
						throw new Exception("Unexpected subject type");
				}
			};

			var subjectUrls = await Task.WhenAll(subjects.Select(s => subject2url(s)));

			return Ok(new UrlSet(subjectUrls));
		}
    }
}
