using MintPlayer.Dtos.Enums;

namespace MintPlayer.Data.Entities.Jobs;

internal class ElasticSearchIndexJob : Job
{
	public Subject Subject { get; set; }
	public ESubjectAction SubjectStatus { get; set; }
}
