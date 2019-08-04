using System;
using System.Collections.Generic;
using MintPlayer.Data.Entities.Interfaces;

namespace MintPlayer.Data.Entities
{
    internal class Person : Subject, ISoftDelete
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Born { get; set; }
        public DateTime? Died { get; set; }

        public List<ArtistPerson> Artists { get; set; }
    }
}
