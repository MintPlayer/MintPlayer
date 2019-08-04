using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MintPlayer.Data.Dtos;
using MintPlayer.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MintPlayer.Data.Repositories
{
    internal class PersonRepository : IPersonRepository
    {
        private IHttpContextAccessor http_context;
        private MintPlayerContext mintplayer_context;
        private UserManager<Entities.User> user_manager;
        private Nest.IElasticClient elastic_client;
        public PersonRepository(IHttpContextAccessor http_context, MintPlayerContext mintplayer_context, UserManager<Entities.User> user_manager, Nest.IElasticClient elastic_client)
        {
            this.http_context = http_context;
            this.mintplayer_context = mintplayer_context;
            this.user_manager = user_manager;
            this.elastic_client = elastic_client;
        }

        public IEnumerable<Dtos.Person> GetPeople(bool include_relations = false)
        {
            if (include_relations)
            {
                var people = mintplayer_context.People
                    .Include(person => person.Artists)
                        .ThenInclude(ap => ap.Artist)
                    .Include(person => person.Media)
                        .ThenInclude(m => m.Type)
                    .Select(person => ToDto(person, true));
                return people;
            }
            else
            {
                var people = mintplayer_context.People
                    .Select(person => ToDto(person, false));
                return people;
            }
        }

        public Dtos.Person GetPerson(int id, bool include_relations = false)
        {
            if (include_relations)
            {
                var person = mintplayer_context.People
                    .Include(p => p.Artists)
                        .ThenInclude(ap => ap.Artist)
                    .Include(p => p.Media)
                        .ThenInclude(m => m.Type)
                    .SingleOrDefault(p => p.Id == id);
                return ToDto(person, true);
            }
            else
            {
                var person = mintplayer_context.People
                    .SingleOrDefault(p => p.Id == id);
                return ToDto(person, false);
            }
        }

        public async Task<Dtos.Person> InsertPerson(Dtos.Person person)
        {
            // Convert to entity
            var entity_person = ToEntity(person, mintplayer_context);

            // Get current user
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            entity_person.UserInsert = user;

            // Add to database
            mintplayer_context.People.Add(entity_person);
            await mintplayer_context.SaveChangesAsync();

            // Index
            var new_person = ToDto(entity_person);
            var index_status = await elastic_client.IndexDocumentAsync(new_person);

            return new_person;
        }

        public async Task<Dtos.Person> UpdatePerson(Dtos.Person person)
        {
            // Find existing person
            var entity_person = mintplayer_context.People.Find(person.Id);

            // Set new properties
            entity_person.FirstName = person.FirstName;
            entity_person.LastName = person.LastName;
            entity_person.Born = person.Born;
            entity_person.Died = person.Died;

            // Get current user
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            entity_person.UserUpdate = user;

            // Update in database
            mintplayer_context.Entry(entity_person).State = EntityState.Modified;

            // Index
            var updated_person = ToDto(entity_person);
            await elastic_client.UpdateAsync<Person>(updated_person, u => u.Doc(updated_person));

            return updated_person;
        }

        public async Task DeletePerson(int person_id)
        {
            // Find existing person
            var person = mintplayer_context.People.Find(person_id);

            // Get current user
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            person.UserDelete = user;

            // Index
            var person_dto = ToDto(person);
            await elastic_client.DeleteAsync<Person>(person_dto);
        }

        public async Task SaveChangesAsync()
        {
            await mintplayer_context.SaveChangesAsync();
        }

        #region Conversion methods
        internal static Dtos.Person ToDto(Entities.Person person, bool include_relations = false)
        {
            if (person == null) return null;
            if (include_relations)
            {
                return new Dtos.Person
                {
                    Id = person.Id,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Born = person.Born,
                    Died = person.Died,

                    Artists = person.Artists.Select(ap => ArtistRepository.ToDto(ap.Artist)).ToList(),
                    Media = person.Media.Select(medium => MediumRepository.ToDto(medium, true)).ToList()
                };
            }
            else
            {
                return new Dtos.Person
                {
                    Id = person.Id,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Born = person.Born,
                    Died = person.Died
                };
            }
        }
        internal static Entities.Person ToEntity(Dtos.Person person, MintPlayerContext mintplayer_context)
        {
            if (person == null) return null;
            var entity_person = new Entities.Person
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Born = person.Born,
                Died = person.Died
            };
            entity_person.Media = person.Media.Select(m => {
                var medium = MediumRepository.ToEntity(m, mintplayer_context);
                medium.Subject = entity_person;
                return medium;
            }).ToList();
            return entity_person;
        }
        #endregion
    }
}
