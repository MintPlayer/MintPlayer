using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MintPlayer.Data.Entities
{
    internal class Subject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public List<Medium> Media { get; set; }
        public List<Like> Likes { get; set; }

        public User UserInsert { get; set; }
        public User UserUpdate { get; set; }
        public User UserDelete { get; set; }
    }
}
