using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Fetcher;

namespace MintPlayer.Data.Services
{
    public interface ISubjectService
    {
        Task<SubjectLikeResult> GetLikes(int subjectId);
        Task<bool?> DoesLike(int subjectId);
        Task<SubjectLikeResult> Like(int subjectId, bool like);
        Task<IEnumerable<Subject>> GetLikedSubjects();

        Task<IEnumerable<Subject>> Suggest(string[] subjects, string search_term, bool include_relations, bool include_invisible_media);
        Task<SearchResults> Search(string[] subjects, string search_term, bool exact, bool include_relations, bool include_invisible_media);
        Task<Fetcher.Dtos.Subject> Fetch(string url);
    }

    internal class SubjectService : ISubjectService
    {
        private ISubjectRepository subjectRepository;
        private readonly IFetcherContainer fetcherContainer;
        public SubjectService(ISubjectRepository subjectRepository, IFetcherContainer fetcherContainer)
        {
            this.subjectRepository = subjectRepository;
            this.fetcherContainer = fetcherContainer;
        }

        public async Task<SubjectLikeResult> GetLikes(int subjectId)
        {
            var likes = await subjectRepository.GetLikes(subjectId);
            bool authenticated; bool? doeslike;
            try
            {
                doeslike = await subjectRepository.DoesLike(subjectId);
                authenticated = true;
            }
            catch (UnauthorizedAccessException)
            {
                doeslike = null;
                authenticated = false;
            }

            return new SubjectLikeResult
            {
                Likes = likes.Item1,
                Dislikes = likes.Item2,
                Like = doeslike,
                Authenticated = authenticated
            };
        }

        public async Task<bool?> DoesLike(int subjectId)
        {
            var doesLike = await subjectRepository.DoesLike(subjectId);
            return doesLike;
        }

        public async Task<SubjectLikeResult> Like(int subjectId, bool like)
        {
            await subjectRepository.Like(subjectId, like);
            await subjectRepository.SaveChangesAsync();
            var likes = await subjectRepository.GetLikes(subjectId);
            var doeslike = await subjectRepository.DoesLike(subjectId);
            return new SubjectLikeResult
            {
                Likes = likes.Item1,
                Dislikes = likes.Item2,
                Like = doeslike,
                Authenticated = true
            };
        }
        
        public Task<IEnumerable<Subject>> GetLikedSubjects()
        {
            return subjectRepository.GetLikedSubjects();
        }

        public async Task<IEnumerable<Subject>> Suggest(string[] subjects, string search_term, bool include_relations, bool include_invisible_media)
        {
            // Still to do:
            // https://devadventures.net/2018/05/03/implementing-autocomplete-and-more-like-this-using-asp-net-core-elasticsearch-and-nest-5-x-part-4-4/

            var valid_subjects = new[] { "artist", "person", "song" };

            if (subjects.FirstOrDefault() == null)
                subjects = valid_subjects;
            else if (subjects.FirstOrDefault() == "all")
                subjects = valid_subjects;
            else if (!subjects.Intersect(valid_subjects).Any())
                subjects = valid_subjects;
            else
                subjects = subjects.Intersect(valid_subjects).ToArray();

            var results = await subjectRepository.Suggest(subjects, search_term, include_relations, include_invisible_media);
            return results;
        }

        public async Task<SearchResults> Search(string[] subjects, string search_term, bool exact, bool include_relations, bool include_invisible_media)
        {
            var valid_subjects = new[] { "artist", "person", "song" };

            if (subjects.FirstOrDefault() == null)
                subjects = valid_subjects;
            else if (subjects.FirstOrDefault() == "all")
                subjects = valid_subjects;
            else if (!subjects.Intersect(valid_subjects).Any())
                subjects = valid_subjects;
            else
                subjects = subjects.Intersect(valid_subjects).ToArray();

            var results = await subjectRepository.Search(subjects, search_term, exact, include_relations, include_invisible_media);
            return new SearchResults
            {
                Artists = results.ToArray().Where(s => s.GetType() == typeof(Artist)).Cast<Artist>().ToList(),
                People = results.ToArray().Where(s => s.GetType() == typeof(Person)).Cast<Person>().ToList(),
                Songs = results.ToArray().Where(s => s.GetType() == typeof(Song)).Cast<Song>().ToList()
            };
        }

        public async Task<Fetcher.Dtos.Subject> Fetch(string url)
        {
            var subject = await fetcherContainer.Fetch(url, true);



            return subject;
        }
    }
}
