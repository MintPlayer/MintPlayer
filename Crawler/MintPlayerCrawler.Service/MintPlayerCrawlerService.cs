using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MintPlayerCrawler.Service
{
    public class MintPlayerCrawlerService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IOptions<MintPlayerCrawlerConfig> _config;
        public MintPlayerCrawlerService(ILogger<MintPlayerCrawlerService> logger, IOptions<MintPlayerCrawlerConfig> config)
        {
            _logger = logger;
            _config = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting daemon: " + _config.Value.DaemonName);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping daemon.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing....");
        }
    }
}
