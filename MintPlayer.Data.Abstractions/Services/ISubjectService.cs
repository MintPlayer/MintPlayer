using System.Collections.Generic;
using System.Threading.Tasks;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Fetcher.Abstractions;

namespace MintPlayer.Data.Abstractions.Services
{
	public interface ISubjectService
	{
		Task<IDictionary<SubjectLookup, Subject[]>> LookupSubject(IEnumerable<SubjectLookup> mediumValues);
		
		Task<SubjectLikeResult> GetLikes(int subjectId);
		Task<bool?> DoesLike(int subjectId);
		Task<SubjectLikeResult> Like(int subjectId, bool like);
		Task<IEnumerable<Subject>> GetLikedSubjects();

		Task<IEnumerable<Subject>> Suggest(string[] subjects, string search_term, bool include_relations);
		Task<SearchResults> Search(string[] subjects, string search_term, bool exact, bool include_relations);
	}
}
