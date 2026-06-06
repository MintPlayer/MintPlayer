using MintPlayer.Dtos.Dtos;
using MintPlayer.Dtos.Enums;

namespace MintPlayer.Data.Abstractions.Dtos.Jobs
{
    public class ElasticSearchIndexJob : Job
    {
        public Subject Subject { get; set; }
        public eSubjectAction SubjectStatus { get; set; }
    }
}
