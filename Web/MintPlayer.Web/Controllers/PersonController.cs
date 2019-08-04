using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Data.Dtos;
using MintPlayer.Data.Repositories.Interfaces;
using MintPlayer.Web.ViewModels.Person;

namespace MintPlayer.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : Controller
    {
        private IPersonRepository personRepository;
        public PersonController(IPersonRepository personRepository)
        {
            this.personRepository = personRepository;
        }

        // GET: api/Person
        [HttpGet]
        public IEnumerable<Person> Get([FromHeader]bool include_relations = false)
        {
            var people = personRepository.GetPeople(include_relations);
            return people.ToList();
        }

        // GET: api/Person/5
        [HttpGet("{id}", Order = 1)]
        public Person Get(int id, [FromHeader]bool include_relations = false)
        {
            var person = personRepository.GetPerson(id, include_relations);
            return person;
        }

        // POST: api/Person
        [HttpPost]
        [Authorize]
        public async Task<Person> Post([FromBody] PersonCreateVM personCreateVM)
        {
            var person = await personRepository.InsertPerson(personCreateVM.Person);
            return person;
        }

        // PUT: api/Person/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task Put(int id, [FromBody] PersonUpdateVM personUpdateVM)
        {
            await personRepository.UpdatePerson(personUpdateVM.Person);
            await personRepository.SaveChangesAsync();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task Delete(int id)
        {
            await personRepository.DeletePerson(id);
            await personRepository.SaveChangesAsync();
        }
    }
}