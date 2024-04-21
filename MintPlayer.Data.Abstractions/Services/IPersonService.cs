using MintPlayer.Dtos.Dtos;

namespace MintPlayer.Data.Abstractions.Services;

public interface IPersonService
{
	Task<Pagination.PaginationResponse<Person>> PagePeople(Pagination.PaginationRequest<Person> request);
	Task<IEnumerable<Person>> GetPeople(bool include_relations);
	Task<Person> GetPerson(int id, bool include_relations);
	Task<Pagination.PaginationResponse<Person>> PageLikedPeople(Pagination.PaginationRequest<Person> request);
	Task<IEnumerable<Person>> GetLikedPeople();
	Task<Person> InsertPerson(Person person);
	Task<Person> UpdatePerson(Person person);
	Task DeletePerson(int person_id);
}
