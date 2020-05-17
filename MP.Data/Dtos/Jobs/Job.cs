using MintPlayer.Data.Enums;

namespace MintPlayer.Data.Dtos.Jobs
{
    public abstract class Job
    {
        public int Id { get; set; }
        public eJobStatus JobStatus { get; set; }

    }
}
