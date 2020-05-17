using MintPlayer.Dtos.Dtos;
using MintPlayer.Dtos.Enums;

namespace MintPlayer.Data.Dtos.Jobs
{
    public class ElasticSearchIndexJob : Job
    {
        public Subject Subject { get; set; }
        public eSubjectAction SubjectStatus { get; set; }
    }
}
