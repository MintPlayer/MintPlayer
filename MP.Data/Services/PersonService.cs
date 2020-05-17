using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MintPlayer.Data.Services
{
	public interface IPersonService
	{
		Task<Pagination.PaginationResponse<Person>> PagePeople(Pagination.PaginationRequest<Person> request);
		Task<IEnumerable<Person>> GetPeople(bool include_relations, bool include_invisible_media);
		Task<Person> GetPerson(int id, bool include_relations, bool include_invisible_media);
		Task<Pagination.PaginationResponse<Person>> PageLikedPeople(Pagination.PaginationRequest<Person> request);
		Task<IEnumerable<Person>> GetLikedPeople();
		Task<Person> InsertPerson(Person person);
		Task<Person> UpdatePerson(Person person);
		Task DeletePerson(int person_id);
	}
	internal class PersonService : IPersonService
	{
		private IPersonRepository personRepository;
		private IMediumRepository mediumRepository;
		public PersonService(IPersonRepository personRepository, IMediumRepository mediumRepository)
		{
			this.personRepository = personRepository;
			this.mediumRepository = mediumRepository;
		}

		public async Task<Pagination.PaginationResponse<Person>> PagePeople(Pagination.PaginationRequest<Person> request)
		{
			var people = await personRepository.PagePeople(request);
			return people;
		}

		public async Task<IEnumerable<Person>> GetPeople(bool include_relations, bool include_invisible_media)
		{
			var people = await personRepository.GetPeople(include_relations, include_invisible_media);
			return people;
		}

		public async Task<Person> GetPerson(int id, bool include_relations, bool include_invisible_media)
		{
			var person = await personRepository.GetPerson(id, include_relations, include_invisible_media);
			return person;
		}

		public Task<Pagination.PaginationResponse<Person>> PageLikedPeople(Pagination.PaginationRequest<Person> request)
		{
			return personRepository.PageLikedPeople(request);
		}

		public Task<IEnumerable<Person>> GetLikedPeople()
		{
			return personRepository.GetLikedPeople();
		}

		public async Task<Person> InsertPerson(Person person)
		{
			var new_person = await personRepository.InsertPerson(person);
			await mediumRepository.StoreMedia(new_person, person.Media);
			return new_person;
		}

		public async Task<Person> UpdatePerson(Person person)
		{
			var updated_person = await personRepository.UpdatePerson(person);
			await mediumRepository.StoreMedia(updated_person, person.Media);
			await personRepository.SaveChangesAsync();
			return updated_person;
		}

		public async Task DeletePerson(int person_id)
		{
			await personRepository.DeletePerson(person_id);
			await personRepository.SaveChangesAsync();
		}
	}
}