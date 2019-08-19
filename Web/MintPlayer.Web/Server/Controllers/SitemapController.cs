﻿using Microsoft.AspNetCore.Mvc;
using MintPlayer.Data.Repositories.Interfaces;
using SitemapXml;
using SitemapXml.SitemapXml.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MintPlayer.Web.Server.Controllers
{
    [Controller]
    [Route("[controller]", Name = "sitemap")]
    public class SitemapController : Controller
    {
        private ISitemapXml sitemapXml;
        private IPersonRepository personRepository;
        private IArtistRepository artistRepository;
        private ISongRepository songRepository;
        public SitemapController(ISitemapXml sitemapXml, IPersonRepository personRepository, IArtistRepository artistRepository, ISongRepository songRepository)
        {
            this.sitemapXml = sitemapXml;
            this.personRepository = personRepository;
            this.artistRepository = artistRepository;
            this.songRepository = songRepository;
        }

        [Produces("application/xml")]
        [HttpGet(Name = "index")]
        public UrlSet Index()
        {
            const int per_page = 100;

            var people = personRepository.GetPeople().ToList();
            var artists = artistRepository.GetArtists().ToList();
            var songs = songRepository.GetSongs().ToList();

            var person_urls = sitemapXml.GetSitemapIndex(people, per_page, (perPage, page) => Url.RouteUrl("subject", new { subject = "person", count = perPage, page }, Request.Scheme));
            var artist_urls = sitemapXml.GetSitemapIndex(artists, per_page, (perPage, page) => Url.RouteUrl("subject", new { subject = "artist", count = perPage, page }, Request.Scheme));
            var song_urls = sitemapXml.GetSitemapIndex(songs, per_page, (perPage, page) => Url.RouteUrl("subject", new { subject = "song", count = perPage, page }, Request.Scheme));

            var all_urls = person_urls.Concat(artist_urls).Concat(song_urls);
            return new UrlSet(all_urls);
        }

        [Produces("application/xml")]
        [HttpGet("{subject}/{count}/{page}", Name = "subject")]
        public UrlSet Sitemap(string subject, int count, int page)
        {
            throw new NotImplementedException();
            //List<Data.Dtos.Subject> subjects;
            //switch (subject.ToLower())
            //{
            //    case "person":
            //        subjects = personRepository.GetPeople().Cast<Data.Dtos.Subject>().ToList();
            //        break;
            //    case "artist":
            //        subjects = artistRepository.GetArtists().Cast<Data.Dtos.Subject>().ToList();
            //        break;
            //    case "song":
            //        subjects = songRepository.GetSongs().Cast<Data.Dtos.Subject>().ToList();
            //        break;
            //    default:
            //        throw new Exception("Invalid subject type");
            //}

            //var items = subjects.Skip((page - 1) * count).Take(count);

            //var urlset = new UrlSet();
            //var urls = items.Select(s => {
            //    var url = new Url
            //    {
            //        //Loc = Url.Action("Get", subject.UcFirst(), new { id = s.Id }, Request.Scheme),
            //        Loc = $"{Request.Scheme}://{Request.Host}/{subject}/{s.Id}",
            //        ChangeFreq = ChangeFreq.Monthly,
            //        LastMod = s.LastEdit,
            //    };
            //    url.Links.Add(new Link
            //    {
            //        Rel = "alternate",
            //        HrefLang = "nl",
            //        Href = $"{Request.Scheme}://{Request.Host}/nl/{subject}/{s.Id}"
            //    });
            //    url.Links.Add(new Link
            //    {
            //        Rel = "alternate",
            //        HrefLang = "fr",
            //        Href = $"{Request.Scheme}://{Request.Host}/fr/{subject}/{s.Id}"
            //    });
            //    return url;
            //});
            //urlset.Urls.AddRange(urls);
            //return urlset;
        }
    }
}
