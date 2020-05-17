using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MintPlayer.Dtos.Dtos
{
	public class Artist : Subject
	{
		public string Name { get; set; }
		public int? YearStarted { get; set; }
		public int? YearQuit { get; set; }

		public List<Person> PastMembers { get; set; }
		public List<Person> CurrentMembers { get; set; }
		public List<Song> Songs { get; set; }

		[JsonIgnore]
		public CompletionField NameSuggest => new CompletionField { Input = new[] { Name } };
	}
}
