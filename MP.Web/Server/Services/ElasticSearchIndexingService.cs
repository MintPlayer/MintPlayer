using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MintPlayer.Data.Repositories.Jobs;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MintPlayer.Web.Server.Services
{
    public class ElasticSearchIndexingService : IHostedService, IDisposable
    {
        private Timer timer;
        //private readonly IElasticSearchJobRepository elasticSearchJobRepository;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IElasticClient elasticClient;
        public ElasticSearchIndexingService(IServiceScopeFactory serviceScopeFactory, IElasticClient elasticClient)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.elasticClient = elasticClient;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async void Dispose(bool disposing)
        {
            if (timer != null)
                await timer.DisposeAsync();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(FindAndRunIndexJob, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private async void FindAndRunIndexJob(object state)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var elasticSearchJobRepository = scope.ServiceProvider.GetRequiredService<IElasticSearchJobRepository>();
                var job = await elasticSearchJobRepository.PopElasticSearchIndexJob();
                if (job != null)
                {
                    try
                    {
                        var subject_type = job.Subject.GetType();
                        if (subject_type == typeof(Dtos.Dtos.Person))
                        {
                            var index_status = await elasticClient.IndexDocumentAsync((Dtos.Dtos.Person)job.Subject);
                            if (!index_status.IsValid)
                            {
                                throw new Exception($"Could not index person with id {job.Subject.Id}");
                            }
                        }
                        else if (subject_type == typeof(Dtos.Dtos.Artist))
                        {
                            var index_status = await elasticClient.IndexDocumentAsync((Dtos.Dtos.Artist)job.Subject);
                            if (!index_status.IsValid)
                            {
                                throw new Exception($"Could not index artist with id {job.Subject.Id}");
                            }
                        }
                        else if (subject_type == typeof(Dtos.Dtos.Song))
                        {
                            var index_status = await elasticClient.IndexDocumentAsync((Dtos.Dtos.Song)job.Subject);
                            if (!index_status.IsValid)
                            {
                                throw new Exception($"Could not index song with id {job.Subject.Id}");
                            }
                        }
                        else
                        {
                            throw new Exception("Unknown subject type");
                        }

                        // Mark job as completed
                        job.JobStatus = Data.Enums.eJobStatus.Completed;
                        await elasticSearchJobRepository.UpdateElasticSearchIndexJob(job);
                        await elasticSearchJobRepository.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        job.JobStatus = Data.Enums.eJobStatus.Error;
                        await elasticSearchJobRepository.UpdateElasticSearchIndexJob(job);
                        await elasticSearchJobRepository.SaveChangesAsync();
                    }
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (timer != null)
                timer.Change(Timeout.Infinite, 0);
        }
    }
}
