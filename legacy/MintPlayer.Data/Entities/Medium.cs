using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MintPlayer.Data.Entities
{
    internal class Medium
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public MediumType Type { get; set; }
        public Subject Subject { get; set; }
        public string Value { get; set; }
    }
}
