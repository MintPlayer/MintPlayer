using Microsoft.EntityFrameworkCore;
using MintPlayer.Data.Dtos.Jobs;
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
        private MintPlayerContext mintplayer_context;
        public ElasticSearchJobRepository(MintPlayerContext mintplayer_context)
        {
            this.mintplayer_context = mintplayer_context;
        }

        public async Task<ElasticSearchIndexJob> InsertElasticSearchIndexJob(ElasticSearchIndexJob job)
        {
            // Convert to entity
            var entity_job = ToEntity(job, mintplayer_context);

            // Add to database
            await mintplayer_context.ElasticSearchIndexJobs.AddAsync(entity_job);
            await mintplayer_context.SaveChangesAsync();

            var new_job = ToDto(entity_job, false, false);
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

            return ToDto(entity_job, false, false);
        }

        public async Task<ElasticSearchIndexJob> PopElasticSearchIndexJob()
        {
            var entity_job = await mintplayer_context.ElasticSearchIndexJobs
                .Include(j => j.Subject)
                .FirstOrDefaultAsync(j => j.Status == Enums.eJobStatus.Queued);
            return ToDto(entity_job, false, false);
        }

        public async Task SaveChangesAsync()
        {
            await mintplayer_context.SaveChangesAsync();
        }

        #region Conversion methods
        internal static ElasticSearchIndexJob ToDto(Entities.Jobs.ElasticSearchIndexJob job, bool include_relations, bool include_invisible_media)
        {
            if (job == null) return null;
            return new ElasticSearchIndexJob
            {
                Id = job.Id,
                JobStatus = job.Status,
                Subject = SubjectRepository.ToDto(job.Subject, include_relations, include_invisible_media),
                SubjectStatus = job.SubjectStatus
            };
        }
        internal static Entities.Jobs.ElasticSearchIndexJob ToEntity(ElasticSearchIndexJob job, MintPlayerContext mintplayer_context)
        {
            if (job == null) return null;
            var entity_job = new Entities.Jobs.ElasticSearchIndexJob
            {
                Id = job.Id,
                Status = job.JobStatus,
                Subject = mintplayer_context.Subjects.Find(job.Subject.Id),
                SubjectStatus = job.SubjectStatus
            };
            return entity_job;
        }
        #endregion
    }
}
