using System.Collections.Generic;

namespace MintPlayer.Dtos.Dtos
{
    public class Playlist
    {
        public int Id { get; set; }
        public User User { get; set; }
        public string Description { get; set; }
        public List<Song> Tracks { get; set; }
    }
}
