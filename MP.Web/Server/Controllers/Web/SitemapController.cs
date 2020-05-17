using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Data.Repositories;
using MintPlayer.AspNetCore.SitemapXml;
using MintPlayer.AspNetCore.SitemapXml.DependencyInjection.Interfaces;
using MintPlayer.Data.Services;
using MintPlayer.Web.Extensions;
using MintPlayer.Dtos.Dtos;
using MintPlayer.AspNetCore.SpaServices.Routing;

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
        public async Task<SitemapIndex> Index()
        {
            const int per_page = 100;

            var people = await personService.GetPeople(false, false);
            var artists = await artistService.GetArtists(false, false);
            var songs = await songService.GetSongs(false, false);

            var person_urls = sitemapXml.GetSitemapIndex(people, per_page, (perPage, page) => Url.RouteUrl("web-sitemap-sitemap", new { subject = "person", count = perPage, page }, Request.Scheme));
            var artist_urls = sitemapXml.GetSitemapIndex(artists, per_page, (perPage, page) => Url.RouteUrl("web-sitemap-sitemap", new { subject = "artist", count = perPage, page }, Request.Scheme));
            var song_urls = sitemapXml.GetSitemapIndex(songs, per_page, (perPage, page) => Url.RouteUrl("web-sitemap-sitemap", new { subject = "song", count = perPage, page }, Request.Scheme));

            return new SitemapIndex(person_urls.Concat(artist_urls).Concat(song_urls));
        }

        // GET: web/Sitemap/{subject}/{count}/{page}
        [Produces("text/xml")]
        [HttpGet("{subject}/{count}/{page}", Name = "web-sitemap-sitemap")]
        public async Task<IActionResult> Sitemap(string subject, int count, int page)
        {
            string[] languages = new string[] { "fr", "nl" };
            IEnumerable<Subject> subjects;
            switch (subject.ToLower())
            {
                case "person":
                    var people = await personService.GetPeople(false, false);
                    subjects = people.Skip((page - 1) * count).Take(count);
                    break;
                case "artist":
                    var artists = await artistService.GetArtists(false, false);
                    subjects = artists.Skip((page - 1) * count).Take(count);
                    break;
                case "song":
                    var songs = await songService.GetSongs(false, false);
                    subjects = songs.Skip((page - 1) * count).Take(count);
                    break;
                default:
                    return NotFound();
            }

            return Ok(new UrlSet(subjects.Select(s =>
            {
                switch (subject)
                {
                    case "person":
                        {
                            return new Url
                            {
                                Loc = spaRouteService.GenerateUrl($"person-show-name", new { id = s.Id, name = s.Text.Slugify() }, HttpContext),
                                ChangeFreq = AspNetCore.SitemapXml.Enums.ChangeFreq.Monthly,
                                LastMod = s.DateUpdate,
                                Links = languages.Select(lang =>
                                    new Link
                                    {
                                        Rel = "alternate",
                                        HrefLang = lang,
                                        Href = spaRouteService.GenerateUrl($"person-show-name", new { id = s.Id, name = s.Text.Slugify(), lang }, HttpContext)
                                    }).ToList()
                            };
                        }
                    case "artist":
                        {
                            return new Url
                            {
                                Loc = spaRouteService.GenerateUrl($"artist-show-name", new { id = s.Id, name = s.Text.Slugify() }, HttpContext),
                                ChangeFreq = AspNetCore.SitemapXml.Enums.ChangeFreq.Monthly,
                                LastMod = s.DateUpdate,
                                Links = languages.Select(lang =>
                                    new Link
                                    {
                                        Rel = "alternate",
                                        HrefLang = lang,
                                        Href = spaRouteService.GenerateUrl($"artist-show-name", new { id = s.Id, name = s.Text.Slugify(), lang }, HttpContext)
                                    }).ToList()
                            };
                        }
                    case "song":
                        {
                            Func<Song, List<Video>> parseSongVideos = (song) => {
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
                            Func<Song, List<Image>> parseSongImages = (song) => {
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

                            return new Url
                            {
                                Loc = spaRouteService.GenerateUrl($"song-show-title", new { id = s.Id, title = s.Text.Slugify() }, HttpContext),
                                ChangeFreq = AspNetCore.SitemapXml.Enums.ChangeFreq.Monthly,
                                LastMod = s.DateUpdate,
                                Links = languages.Select(lang =>
                                    new Link
                                    {
                                        Rel = "alternate",
                                        HrefLang = lang,
                                        Href = spaRouteService.GenerateUrl($"song-show-title", new { id = s.Id, title = s.Text.Slugify(), lang }, HttpContext)
                                    }).ToList(),
                                Videos = parseSongVideos((Song)s),
                                Images = parseSongImages((Song)s)
                            };
                        }
                    default:
                        throw new Exception("Unexpected subject type");
                }
            })));
        }
    }
}