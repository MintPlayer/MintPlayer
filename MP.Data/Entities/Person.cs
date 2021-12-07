using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MintPlayer.Data.Entities.Interfaces;

namespace MintPlayer.Data.Entities
{
	[Table("People", Schema = "mintplay")]
	internal class Person : Subject, ISoftDelete
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Born { get; set; }
        public DateTime? Died { get; set; }
        
		[NotMapped]
        public override string Text => $"{FirstName} {LastName}";

        public List<ArtistPerson> Artists { get; set; }
    }
}
