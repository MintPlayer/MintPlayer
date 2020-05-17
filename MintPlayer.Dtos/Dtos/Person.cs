using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MintPlayer.Dtos.Dtos
{
	public class Person : Subject
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime? Born { get; set; }
		public DateTime? Died { get; set; }

		public List<Artist> Artists { get; set; }

		[JsonIgnore]
		public CompletionField NameSuggest => new CompletionField { Input = new[] { $"{FirstName} {LastName}" } };
	}
}
