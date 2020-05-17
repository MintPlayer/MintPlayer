using System.Collections.Generic;
using System.Linq;

namespace MintPlayer.Dtos.Dtos
{
    public class SearchResults
    {
        public List<Person> People { get; set; }
        public List<Artist> Artists { get; set; }
        public List<Song> Songs { get; set; }

        public int Count
        {
            get
            {
                int people = 0, artists = 0, songs = 0;
                if (People != null) people = People.Count;
                if (Artists != null) artists = Artists.Count;
                if (Songs != null) songs = Songs.Count;

                return people + artists + songs;
            }
        }

        public Subject First()
        {
            if (People.Any()) return People.First();
            if (Artists.Any()) return Artists.First();
            if (Songs.Any()) return Songs.First();

            return null;
        }
    }
}
