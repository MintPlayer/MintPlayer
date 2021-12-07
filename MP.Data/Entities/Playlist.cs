using MintPlayer.Dtos.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MintPlayer.Data.Entities
{
	[Table("Playlists", Schema = "mintplay")]
    internal class Playlist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public User User { get; set; }
        public string Description { get; set; }
        public List<PlaylistSong> Tracks { get; set; }
        public ePlaylistAccessibility Accessibility { get; set; }

        public bool IsDeleted { get; set; }
    }
}
