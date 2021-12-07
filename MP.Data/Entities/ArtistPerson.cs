using System.ComponentModel.DataAnnotations.Schema;

namespace MintPlayer.Data.Entities
{
	[Table("ArtistPerson", Schema = "mintplay")]
	internal class ArtistPerson
	{
		public ArtistPerson()
		{
		}

		public ArtistPerson(Artist artist, Person person) : this()
		{
			Artist = artist;
			ArtistId = artist.Id;
			Person = person;
			PersonId = person.Id;
		}

		public int ArtistId { get; set; }
		public Artist Artist { get; set; }

		public int PersonId { get; set; }
		public Person Person { get; set; }

		public bool Active { get; set; }
	}
}
