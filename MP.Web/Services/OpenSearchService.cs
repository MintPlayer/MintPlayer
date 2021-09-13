using AspNetCoreOpenSearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MintPlayer.AspNetCore.SpaServices.Routing;
using MintPlayer.Data.Repositories;
using MintPlayer.Data.Services;
using MintPlayer.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MintPlayer.Web.Services
{
    public class OpenSearchService : IOpenSearchService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ISpaRouteService spaRouteService;
        public OpenSearchService(IServiceProvider serviceProvider, ISpaRouteService spaRouteService)
        {
            this.serviceProvider = serviceProvider;
            this.spaRouteService = spaRouteService;
        }

        public async Task<RedirectResult> PerformSearch(string searchTerms)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var subjectRepository = scope.ServiceProvider.GetService<ISubjectService>();

                var exact_matches = await subjectRepository.Search(new[] { "person", "artist", "song" }, searchTerms, true, false);
                if (exact_matches.Count == 1)
                {
                    var subject_type = exact_matches.First().GetType();
                    string subject_url;
                    if (subject_type == typeof(Dtos.Dtos.Person))
                    {
                        var person = exact_matches.First();
                        subject_url = spaRouteService.GenerateUrl("person-show-name", new { id = person.Id, name = person.Text.Slugify() });
                    }
                    else if (subject_type == typeof(Dtos.Dtos.Artist))
                    {
                        var artist = exact_matches.First();
                        subject_url = spaRouteService.GenerateUrl("artist-show-name", new { id = artist.Id, name = artist.Text.Slugify() });
                    }
                    else if (subject_type == typeof(Dtos.Dtos.Song))
                    {
                        var song = exact_matches.First();
                        subject_url = spaRouteService.GenerateUrl("song-show-title", new { id = song.Id, title = song.Text.Slugify() });
                    }
                    else
                        throw new Exception("The specified type is not a Subject");


                    return new RedirectResult(subject_url);
                }
                else
                {
                    return new RedirectResult($"/search/{searchTerms}");
                }
            }
        }

        public async Task<IEnumerable<string>> ProvideSuggestions(string searchTerms)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var subjectService = scope.ServiceProvider.GetService<ISubjectService>();
                
                var valid_subjects = new[] { "artist", "person", "song" };

                var results = await subjectService.Suggest(valid_subjects, searchTerms, false);
                return results.ToArray().Select(s => s.Text);
            }
        }
    }
}
