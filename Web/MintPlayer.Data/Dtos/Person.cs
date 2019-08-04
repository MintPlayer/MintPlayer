using System;

namespace MintPlayer.Data.Dtos
{
    public class Person : Subject
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Born { get; set; }
        public DateTime? Died { get; set; }

        public string Text => $"{FirstName} {LastName}";

        //public List<Artist> Artists { get; set; }
    }
}
