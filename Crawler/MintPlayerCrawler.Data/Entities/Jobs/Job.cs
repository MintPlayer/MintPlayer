using MintPlayerCrawler.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MintPlayerCrawler.Data.Entities.Jobs
{
    internal class Job
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public eJobStatus Status { get; set; }
    }
}
