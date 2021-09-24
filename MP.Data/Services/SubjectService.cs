using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using MintPlayer.Data.Abstractions.Services;

namespace MintPlayer.Data.Services
{
	internal class SubjectService : ISubjectService
	{
		private readonly ISubjectRepository subjectRepository;
		private readonly UserManager<Entities.User> userManager;
		private readonly IHttpContextAccessor httpContextAccessor;
		public SubjectService(ISubjectRepository subjectRepository, UserManager<Entities.User> userManager, IHttpContextAccessor httpContextAccessor)
		{
			this.subjectRepository = subjectRepository;
			this.userManager = userManager;
			this.httpContextAccessor = httpContextAccessor;
		}

		public async Task<IDictionary<string, Subject[]>> GetByMedium(params string[] mediumValues)
		{
			var subjects = await subjectRepository.GetByMedium(mediumValues);
			return subjects;
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

		public async Task<IEnumerable<Subject>> Suggest(string[] subjects, string search_term, bool include_relations)
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

			var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
			var isAdmin = user == null ? false : await userManager.IsInRoleAsync(user, "Administrator");
			var results = await subjectRepository.Suggest(subjects, search_term, include_relations, isAdmin);
			return results;
		}

		public async Task<SearchResults> Search(string[] subjects, string search_term, bool exact, bool include_relations)
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

			var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
			var isAdmin = user == null ? false : await userManager.IsInRoleAsync(user, "Administrator");
			var results = await subjectRepository.Search(subjects, search_term, exact, include_relations, isAdmin);
			return new SearchResults
			{
				Artists = results.ToArray().Where(s => s.GetType() == typeof(Artist)).Cast<Artist>().ToList(),
				People = results.ToArray().Where(s => s.GetType() == typeof(Person)).Cast<Person>().ToList(),
				Songs = results.ToArray().Where(s => s.GetType() == typeof(Song)).Cast<Song>().ToList()
			};
		}
	}
}
