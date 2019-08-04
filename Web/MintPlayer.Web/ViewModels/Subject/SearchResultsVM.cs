using System.Collections.Generic;

namespace MintPlayer.Web.ViewModels.Subject
{
    public class SearchResultsVM
    {
        public List<Data.Dtos.Person> People { get; set; }
        public List<Data.Dtos.Artist> Artists { get; set; }
        public List<Data.Dtos.Song> Songs { get; set; }
    }
}
