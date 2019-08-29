using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Data.Dtos;
using MintPlayer.Data.Repositories.Interfaces;
using MintPlayer.Web.Server.ViewModels.Subject;
using OpenSearch;

namespace MintPlayer.Web.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectController : Controller
    {
        private ISubjectRepository subjectRepository;
        public SubjectController(ISubjectRepository subjectRepository)
        {
            this.subjectRepository = subjectRepository;
        }

        [HttpGet("search/suggest/{subjects_concat}/{search_term}", Name = "search-suggest")]
        public async Task<IEnumerable<Subject>> Suggest([FromRoute]string subjects_concat, [FromRoute]string search_term)
        {
            var subjects = subjects_concat.Split('-');
            var valid_subjects = new[] { "artist", "person", "song" };

            if (subjects.FirstOrDefault() == null)
                subjects = valid_subjects;
            else if (subjects.FirstOrDefault() == "all")
                subjects = valid_subjects;
            else if (!subjects.Intersect(valid_subjects).Any())
                subjects = valid_subjects;
            else
                subjects = subjects.Intersect(valid_subjects).ToArray();

            var results = await subjectRepository.Suggest(subjects, search_term);
            return results.ToList();
        }

        [HttpGet("search/{subjects_concat}/{search_term}", Name = "search-action")]
        public async Task<SearchResultsVM> Search([FromRoute]string subjects_concat, [FromRoute]string search_term)
        {
            var subjects = subjects_concat.Split('-');
            var valid_subjects = new[] { "artist", "person", "song" };

            if (subjects.FirstOrDefault() == null)
                subjects = valid_subjects;
            else if (subjects.FirstOrDefault() == "all")
                subjects = valid_subjects;
            else if (!subjects.Intersect(valid_subjects).Any())
                subjects = valid_subjects;
            else
                subjects = subjects.Intersect(valid_subjects).ToArray();

            var results = await subjectRepository.Search(subjects, search_term, true);
            return new SearchResultsVM
            {
                Artists = results.Where(s => s.GetType() == typeof(Artist)).Cast<Artist>().ToList(),
                People = results.Where(s => s.GetType() == typeof(Person)).Cast<Person>().ToList(),
                Songs = results.Where(s => s.GetType() == typeof(Song)).Cast<Song>().ToList()
            };
        }

        [Produces("application/json")]
        [HttpGet("opensearch/suggest/{search_term}" , Name = "opensearch-suggest")]
        public async Task<IEnumerable<object>> OpenSearchSuggest([FromRoute]string search_term)
        {
            var results = await subjectRepository.Suggest(new[] { "artist", "person", "song" }, search_term);
            return new object[] { search_term, results.Select(r => r.Text) };
        }

        [HttpGet("opensearch/redirect/{search_term}", Name = "opensearch-redirect")]
        public async Task<IActionResult> OpenSearchRedirect([FromRoute]string search_term)
        {
            var results = await subjectRepository.Search(new[] { "artist", "person", "song" }, search_term, false);
            if(results.Count != 1)
            {
                return Redirect($"/search/all/{search_term}");
            }
            else
            {
                var subject = results.First();
                var subject_type = subject.GetType();
                if(subject_type == typeof(Person))
                {
                    return Redirect($"/person/{subject.Id}");
                }
                else if (subject_type == typeof(Artist))
                {
                    return Redirect($"/artist/{subject.Id}");
                }
                else if (subject.GetType() == typeof(Song))
                {
                    return Redirect($"/song/{subject.Id}");
                }
                else
                {
                    throw new Exception("Unexpected exception. Type should always be Person, Artist, Song");
                }
            }
        }

        [HttpGet("{subject_id}/likes")]
        public async Task<SubjectLikeVM> Likes([FromRoute]int subject_id)
        {
            var likes = await subjectRepository.GetLikes(subject_id);
            bool authenticated; bool? doeslike;
            try
            {
                doeslike = await subjectRepository.DoesLike(subject_id);
                authenticated = true;
            }
            catch (UnauthorizedAccessException)
            {
                doeslike = null;
                authenticated = false;
            }

            return new SubjectLikeVM
            {
                Likes = likes.Item1,
                Dislikes = likes.Item2,
                Like = doeslike,
                Authenticated = authenticated
            };
        }

        [Authorize]
        [HttpPost("{subject_id}/likes")]
        public async Task<SubjectLikeVM> Like([FromRoute]int subject_id, [FromBody]bool like)
        {
            await subjectRepository.Like(subject_id, like);
            await subjectRepository.SaveChangesAsync();
            var likes = await subjectRepository.GetLikes(subject_id);
            var doeslike = await subjectRepository.DoesLike(subject_id);
            return new SubjectLikeVM
            {
                Likes = likes.Item1,
                Dislikes = likes.Item2,
                Like = doeslike,
                Authenticated = true
            };
        }
    }
}