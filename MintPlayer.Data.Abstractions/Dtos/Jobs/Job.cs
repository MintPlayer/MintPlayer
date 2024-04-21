using MintPlayer.Data.Abstractions.Enums;

namespace MintPlayer.Data.Abstractions.Dtos.Jobs;

public abstract class Job
{
	public int Id { get; set; }
	public EJobStatus JobStatus { get; set; }
}
