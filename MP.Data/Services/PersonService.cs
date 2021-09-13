using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace MintPlayer.Data.Services
{
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
	internal class PersonService : IPersonService
	{
		private IPersonRepository personRepository;
		private IMediumRepository mediumRepository;
		private readonly UserManager<Entities.User> userManager;
		private readonly IHttpContextAccessor httpContextAccessor;
		public PersonService(IPersonRepository personRepository, IMediumRepository mediumRepository, UserManager<Entities.User> userManager, IHttpContextAccessor httpContextAccessor)
		{
			this.personRepository = personRepository;
			this.mediumRepository = mediumRepository;
			this.userManager = userManager;
			this.httpContextAccessor = httpContextAccessor;
		}

		public async Task<Pagination.PaginationResponse<Person>> PagePeople(Pagination.PaginationRequest<Person> request)
		{
			var people = await personRepository.PagePeople(request);
			return people;
		}

		public async Task<IEnumerable<Person>> GetPeople(bool include_relations)
		{
			var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
			var isAdmin = user == null ? false : await userManager.IsInRoleAsync(user, "Administrator");
			var people = await personRepository.GetPeople(include_relations, isAdmin);
			return people;
		}

		public async Task<Person> GetPerson(int id, bool include_relations)
		{
			var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
			var isAdmin = user == null ? false : await userManager.IsInRoleAsync(user, "Administrator");
			var person = await personRepository.GetPerson(id, include_relations, isAdmin);
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
