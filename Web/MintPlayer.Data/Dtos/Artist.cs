using System.Collections.Generic;

namespace MintPlayer.Data.Dtos
{
    public class Artist : Subject
    {
        public string Name { get; set; }
        public int? YearStarted { get; set; }
        public int? YearQuit { get; set; }

        public string Text => Name;

        public List<Person> PastMembers { get; set; }
        public List<Person> CurrentMembers { get; set; }
    }
}
