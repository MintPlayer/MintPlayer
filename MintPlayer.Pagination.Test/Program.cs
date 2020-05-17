using Newtonsoft.Json;
using System;

namespace MintPlayer.Pagination.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var request = new PaginationRequest<Person>();
            request.PerPage = 20;
            request.Page = 2;
            request.SortProperty = nameof(Person.Born);

            var result = JsonConvert.SerializeObject(request);
        }
    }

    class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Born { get; set; }
        public DateTime Died { get; set; }
    }
}
