using MintPlayerCrawler.Data.Repositories.Interfaces;
using System.Threading.Tasks;

namespace MintPlayerCrawler.Data.Repositories
{
    internal class RequestJobRepository : IRequestJobRepository
    {
        private MintPlayerCrawlerContext mintplayer_crawler_context;
        public RequestJobRepository(MintPlayerCrawlerContext mintplayer_crawler_context)
        {
            this.mintplayer_crawler_context = mintplayer_crawler_context;
        }

        public async Task<Dtos.Jobs.RequestJob> CreateRequestJob(Dtos.Jobs.RequestJob job)
        {
            var entity_job = ToEntity(job);

            // Force status to Pending
            entity_job.Status = Enums.eJobStatus.Pending;

            await mintplayer_crawler_context.RequestJobs.AddAsync(entity_job);
            await mintplayer_crawler_context.SaveChangesAsync();

            return ToDto(entity_job);
        }

        #region Conversion methods
        internal static Dtos.Jobs.RequestJob ToDto(Entities.Jobs.RequestJob job)
        {
            if (job == null) return null;

            return new Dtos.Jobs.RequestJob
            {
                Id = job.Id,
                Url = job.Url,
                Status = job.Status
            };
        }
        internal static Entities.Jobs.RequestJob ToEntity(Dtos.Jobs.RequestJob job)
        {
            if (job == null) return null;

            return new Entities.Jobs.RequestJob
            {
                Id = job.Id,
                Url = job.Url,
                Status = job.Status
            };
        }
        #endregion
    }
}
