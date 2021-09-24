using System.Collections.Generic;
using System.Threading.Tasks;
using MintPlayer.Dtos.Dtos;

namespace MintPlayer.Data.Abstractions.Services
{
	public interface ISubjectService
	{
		Task<IDictionary<string, Subject[]>> GetByMedium(params string[] mediumValues);
		
		Task<SubjectLikeResult> GetLikes(int subjectId);
		Task<bool?> DoesLike(int subjectId);
		Task<SubjectLikeResult> Like(int subjectId, bool like);
		Task<IEnumerable<Subject>> GetLikedSubjects();

		Task<IEnumerable<Subject>> Suggest(string[] subjects, string search_term, bool include_relations);
		Task<SearchResults> Search(string[] subjects, string search_term, bool exact, bool include_relations);
	}
}
