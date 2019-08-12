using Nest;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MintPlayer.Data.Dtos
{
    public class Person : Subject
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Born { get; set; }
        public DateTime? Died { get; set; }

        public string Text => $"{FirstName} {LastName}";

        [JsonIgnore]
        [Completion]
        public CompletionField NameSuggest => new CompletionField { Input = new[] { $"{FirstName} {LastName}" } };

        public List<Artist> Artists { get; set; }
    }
}
