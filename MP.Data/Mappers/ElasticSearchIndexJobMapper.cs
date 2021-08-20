namespace MintPlayer.Data.Mappers
{
    internal interface IElasticSearchIndexJobMapper
    {
        Dtos.Jobs.ElasticSearchIndexJob Entity2Dto(Entities.Jobs.ElasticSearchIndexJob job, bool include_invisible_media, bool include_relations = false);
        Entities.Jobs.ElasticSearchIndexJob Dto2Entity(Dtos.Jobs.ElasticSearchIndexJob job, MintPlayerContext mintplayer_context);
    }

    internal class ElasticSearchIndexJobMapper : IElasticSearchIndexJobMapper
    {
        private readonly ISubjectMapper subjectMapper;
        public ElasticSearchIndexJobMapper(ISubjectMapper subjectMapper)
        {
            this.subjectMapper = subjectMapper;
        }

        public Dtos.Jobs.ElasticSearchIndexJob Entity2Dto(Entities.Jobs.ElasticSearchIndexJob job, bool include_invisible_media, bool include_relations = false)
        {
            if (job == null)
            {
                return null;
            }

            return new Dtos.Jobs.ElasticSearchIndexJob
            {
                Id = job.Id,
                JobStatus = job.Status,
                Subject = subjectMapper.Entity2Dto(job.Subject, include_invisible_media),
                SubjectStatus = job.SubjectStatus,
            };
        }

        public Entities.Jobs.ElasticSearchIndexJob Dto2Entity(Dtos.Jobs.ElasticSearchIndexJob job, MintPlayerContext mintplayer_context)
        {
            if (job == null)
            {
                return null;
            }

            var entity_job = new Entities.Jobs.ElasticSearchIndexJob
            {
                Id = job.Id,
                Status = job.JobStatus,
                Subject = mintplayer_context.Subjects.Find(job.Subject.Id),
                SubjectStatus = job.SubjectStatus,
            };

            return entity_job;
        }
    }
}
