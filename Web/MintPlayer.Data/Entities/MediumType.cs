using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MintPlayer.Data.Enums;
using MintPlayer.Data.Entities.Interfaces;

namespace MintPlayer.Data.Entities
{
    internal class MediumType : ISoftDelete
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Description { get; set; }
        public ePlayerType PlayerType { get; set; }

        public User UserInsert { get; set; }
        public User UserUpdate { get; set; }
        public User UserDelete { get; set; }
    }
}
