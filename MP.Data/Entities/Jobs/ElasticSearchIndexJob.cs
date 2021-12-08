using System.ComponentModel.DataAnnotations.Schema;
using MintPlayer.Dtos.Enums;

namespace MintPlayer.Data.Entities.Jobs
{
	[Table("ElasticSearchIndexJobs")]
    internal class ElasticSearchIndexJob : Job
    {
        public Subject Subject { get; set; }
        public eSubjectAction SubjectStatus { get; set; }
    }
}
