using Microsoft.EntityFrameworkCore;
using MintPlayer.Data.Abstractions.Dtos.Jobs;
using MintPlayer.Data.Mappers;
using System.Linq;
using System.Threading.Tasks;

namespace MintPlayer.Data.Repositories.Jobs
{
    public interface IElasticSearchJobRepository
    {
        Task<ElasticSearchIndexJob> InsertElasticSearchIndexJob(ElasticSearchIndexJob job);
        Task<ElasticSearchIndexJob> UpdateElasticSearchIndexJob(ElasticSearchIndexJob job);
        Task<ElasticSearchIndexJob> PopElasticSearchIndexJob();
        Task SaveChangesAsync();
    }
    internal class ElasticSearchJobRepository : IElasticSearchJobRepository
    {
        private readonly MintPlayerContext mintplayer_context;
        private readonly IElasticSearchIndexJobMapper elasticSearchIndexJobMapper;
        public ElasticSearchJobRepository(
            MintPlayerContext mintplayer_context,
            IElasticSearchIndexJobMapper elasticSearchIndexJobMapper)
        {
            this.mintplayer_context = mintplayer_context;
            this.elasticSearchIndexJobMapper = elasticSearchIndexJobMapper;
        }

        public async Task<ElasticSearchIndexJob> InsertElasticSearchIndexJob(ElasticSearchIndexJob job)
        {
            // Convert to entity
            var entity_job = elasticSearchIndexJobMapper.Dto2Entity(job, mintplayer_context);

            // Add to database
            await mintplayer_context.ElasticSearchIndexJobs.AddAsync(entity_job);
            await mintplayer_context.SaveChangesAsync();

            var new_job = elasticSearchIndexJobMapper.Entity2Dto(entity_job, false, false);
            return new_job;
        }

        public async Task<ElasticSearchIndexJob> UpdateElasticSearchIndexJob(ElasticSearchIndexJob job)
        {
            // Get entity from database
            var entity_job = await mintplayer_context.ElasticSearchIndexJobs.FindAsync(job.Id);

            // Set properties
            entity_job.Status = job.JobStatus;

            // Update
            mintplayer_context.ElasticSearchIndexJobs.Update(entity_job);

            return elasticSearchIndexJobMapper.Entity2Dto(entity_job, false, false);
        }

        public async Task<ElasticSearchIndexJob> PopElasticSearchIndexJob()
        {
            var entity_job = await mintplayer_context.ElasticSearchIndexJobs
                .Include(j => j.Subject)
                .FirstOrDefaultAsync(j => j.Status == Abstractions.Enums.EJobStatus.Queued);
            return elasticSearchIndexJobMapper.Entity2Dto(entity_job, false, false);
        }

        public async Task SaveChangesAsync()
        {
            await mintplayer_context.SaveChangesAsync();
        }
    }
}
