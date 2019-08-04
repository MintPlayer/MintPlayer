using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        public PersonRepository(IHttpContextAccessor http_context, MintPlayerContext mintplayer_context, UserManager<Entities.User> user_manager)
        {
            this.http_context = http_context;
            this.mintplayer_context = mintplayer_context;
            this.user_manager = user_manager;
        }

        public IEnumerable<Dtos.Person> GetPeople(bool include_relations = false)
        {
            if (include_relations)
            {
                var people = mintplayer_context.People
                    .Include(person => person.Artists)
                        .ThenInclude(ap => ap.Artist)
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

            var new_person = ToDto(entity_person);
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

            var new_person = ToDto(entity_person);
            return new_person;
        }

        public async Task DeletePerson(int person_id)
        {
            // Find existing person
            var person = mintplayer_context.People.Find(person_id);

            // Get current user
            var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
            person.UserDelete = user;
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

                    Artists = person.Artists.Select(ap => ArtistRepository.ToDto(ap.Artist)).ToList()
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
            return entity_person;
        }
        #endregion
    }
}
