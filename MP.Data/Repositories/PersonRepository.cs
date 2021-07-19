using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MintPlayer.Dtos.Dtos;
using System;
using MintPlayer.Data.Extensions;
using MintPlayer.Data.Helpers;
using MintPlayer.Data.Mappers;

namespace MintPlayer.Data.Repositories
{
	internal interface IPersonRepository
	{
		Task<Pagination.PaginationResponse<Person>> PagePeople(Pagination.PaginationRequest<Person> request);
		Task<IEnumerable<Person>> GetPeople(bool include_relations, bool include_invisible_media);
		Task<Person> GetPerson(int id, bool include_relations, bool include_invisible_media);
		Task<Pagination.PaginationResponse<Person>> PageLikedPeople(Pagination.PaginationRequest<Person> request);
		Task<IEnumerable<Person>> GetLikedPeople();
		Task<Person> InsertPerson(Person person);
		Task<Person> UpdatePerson(Person person);
		Task DeletePerson(int person_id);
		Task SaveChangesAsync();
	}
	internal class PersonRepository : IPersonRepository
	{
		private readonly IHttpContextAccessor http_context;
		private readonly MintPlayerContext mintplayer_context;
		private readonly UserManager<Entities.User> user_manager;
		private readonly SubjectHelper subject_helper;
        private readonly IPersonMapper personMapper;
        private readonly Jobs.IElasticSearchJobRepository elasticSearchJobRepository;
		public PersonRepository(
			IHttpContextAccessor http_context,
			MintPlayerContext mintplayer_context,
			UserManager<Entities.User> user_manager,
			SubjectHelper subject_helper,
			IPersonMapper personMapper,
			Jobs.IElasticSearchJobRepository elasticSearchJobRepository)
		{
			this.http_context = http_context;
			this.mintplayer_context = mintplayer_context;
			this.user_manager = user_manager;
            this.subject_helper = subject_helper;
            this.personMapper = personMapper;
            this.elasticSearchJobRepository = elasticSearchJobRepository;
		}

		public async Task<Pagination.PaginationResponse<Person>> PagePeople(Pagination.PaginationRequest<Person> request)
		{
			var people = mintplayer_context.People;

			// 1) Sort
			var ordered_people = request.SortDirection == System.ComponentModel.ListSortDirection.Descending
				? people.OrderByDescending(request.SortProperty)
				: people.OrderBy(request.SortProperty);

			// 2) Page
			var paged_people = ordered_people
				.Skip((request.Page - 1) * request.PerPage)
				.Take(request.PerPage);

			// 3) Convert to DTO
			var dto_people = await paged_people.Select(person => personMapper.Entity2Dto(person, false, false)).ToListAsync();

			var count_people = await people.CountAsync();
			return new Pagination.PaginationResponse<Person>(request, count_people, dto_people);
		}

		public Task<IEnumerable<Person>> GetPeople(bool include_relations, bool include_invisible_media)
		{
			if (include_relations)
			{
				var people = mintplayer_context.People
					.Include(person => person.Artists)
						.ThenInclude(ap => ap.Artist)
					.Include(person => person.Media)
						.ThenInclude(m => m.Type)
					.Include(person => person.Tags)
						.ThenInclude(st => st.Tag)
					.Select(person => personMapper.Entity2Dto(person, include_relations, include_invisible_media));
				return Task.FromResult<IEnumerable<Person>>(people);
			}
			else
			{
				var people = mintplayer_context.People
					.Select(person => personMapper.Entity2Dto(person, include_relations, include_invisible_media));
				return Task.FromResult<IEnumerable<Person>>(people);
			}
		}

		public async Task<Person> GetPerson(int id, bool include_relations, bool include_invisible_media)
		{
			if (include_relations)
			{
				var person = await mintplayer_context.People
					.Include(p => p.Artists)
						.ThenInclude(ap => ap.Artist)
					.Include(p => p.Media)
						.ThenInclude(m => m.Type)
					.Include(p => p.Tags)
						.ThenInclude(st => st.Tag)
							.ThenInclude(t => t.Category)
					.SingleOrDefaultAsync(p => p.Id == id);
				return personMapper.Entity2Dto(person, include_relations, include_invisible_media);
			}
			else
			{
				var person = await mintplayer_context.People
					.SingleOrDefaultAsync(p => p.Id == id);
				return personMapper.Entity2Dto(person, include_relations, include_invisible_media);
			}
		}

		public async Task<Pagination.PaginationResponse<Person>> PageLikedPeople(Pagination.PaginationRequest<Person> request)
		{
			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			if (user == null) throw new UnauthorizedAccessException();

			var people = mintplayer_context.People;

			// 1) Filter
			var filtered_people = people
				.Where(s => s.Likes.Any(l => l.User == user && l.DoesLike));

			// 2) Sort
			var ordered_people = request.SortDirection == System.ComponentModel.ListSortDirection.Descending
				? filtered_people.OrderByDescending(request.SortProperty)
				: filtered_people.OrderBy(request.SortProperty);

			// 3) Page
			var paged_people = ordered_people
				.Skip((request.Page - 1) * request.PerPage)
				.Take(request.PerPage);

			// 4) Convert to DTO
			var dto_people = paged_people.Select(person => personMapper.Entity2Dto(person, false, false));

			var count_people = await filtered_people.CountAsync();
			return new Pagination.PaginationResponse<Person>(request, count_people, dto_people);
		}

		public async Task<IEnumerable<Person>> GetLikedPeople()
		{
			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			if (user == null) throw new UnauthorizedAccessException();

			var people = mintplayer_context.People
				.Where(s => s.Likes.Any(l => l.User == user && l.DoesLike))
				.Select(s => personMapper.Entity2Dto(s, false, false));

			return people;
		}

		public async Task<Person> InsertPerson(Person person)
		{
			// Convert to entity
			var entity_person = personMapper.Dto2Entity(person, mintplayer_context);

			// Get current user
			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			entity_person.UserInsert = user;
			entity_person.DateInsert = DateTime.Now;

			// Add to database
			await mintplayer_context.People.AddAsync(entity_person);
			await mintplayer_context.SaveChangesAsync();

			var new_person = personMapper.Entity2Dto(entity_person, false, false);
			var job = await elasticSearchJobRepository.InsertElasticSearchIndexJob(new Data.Dtos.Jobs.ElasticSearchIndexJob
			{
				Subject = new_person,
				SubjectStatus = MintPlayer.Dtos.Enums.eSubjectAction.Added,
				JobStatus = Data.Enums.eJobStatus.Queued
			});

			return new_person;
		}

		public async Task<Person> UpdatePerson(Person person)
		{
			// Find existing person
			var entity_person = await mintplayer_context.People
				.Include(p => p.Tags)
				.SingleOrDefaultAsync(p => p.Id == person.Id);

			// Set new properties
			entity_person.FirstName = person.FirstName;
			entity_person.LastName = person.LastName;
			entity_person.Born = person.Born;
			entity_person.Died = person.Died;

			IEnumerable<Entities.SubjectTag> tags_to_add, tags_to_remove;
			subject_helper.CalculateUpdatedTags(entity_person, person, mintplayer_context, out tags_to_add, out tags_to_remove);
			foreach (var item in tags_to_remove)
				mintplayer_context.Remove(item);
			foreach (var item in tags_to_add)
				await mintplayer_context.AddAsync(item);

			// Get current user
			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			entity_person.UserUpdate = user;
			entity_person.DateUpdate = DateTime.Now;

			// Update in database
			mintplayer_context.Entry(entity_person).State = EntityState.Modified;
			
			var updated_person = personMapper.Entity2Dto(entity_person, false, false);
			var job = await elasticSearchJobRepository.InsertElasticSearchIndexJob(new Data.Dtos.Jobs.ElasticSearchIndexJob
			{
				Subject = updated_person,
				SubjectStatus = MintPlayer.Dtos.Enums.eSubjectAction.Added,
				JobStatus = Data.Enums.eJobStatus.Queued
			});

			return updated_person;
		}

		public async Task DeletePerson(int person_id)
		{
			// Find existing person
			var person = await mintplayer_context.People.FindAsync(person_id);

			// Get current user
			var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
			person.UserDelete = user;
			person.DateDelete = DateTime.Now;

			var deleted_person = personMapper.Entity2Dto(person, false, false);
			var job = await elasticSearchJobRepository.InsertElasticSearchIndexJob(new Data.Dtos.Jobs.ElasticSearchIndexJob
			{
				Subject = deleted_person,
				SubjectStatus = MintPlayer.Dtos.Enums.eSubjectAction.Deleted,
				JobStatus = Data.Enums.eJobStatus.Queued
			});
		}

		public async Task SaveChangesAsync()
		{
			await mintplayer_context.SaveChangesAsync();
		}
	}
}
