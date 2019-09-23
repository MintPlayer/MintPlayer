using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MintPlayerCrawler.Data.Options;

namespace MintPlayerCrawler.Web.Services
{
    public class MintPlayerCrawlerService : IHostedService, IDisposable
    {
        private readonly ILogger logger;
        private readonly IOptions<MintPlayerCrawlerOptions> crawler_options;
        private CancellationTokenSource cancellationTokenSource;
        private bool is_running;
        public MintPlayerCrawlerService(ILogger<MintPlayerCrawlerService> logger, IOptions<MintPlayerCrawlerOptions> crawler_options)
        {
            this.logger = logger;
            this.crawler_options = crawler_options;
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            is_running = true;
            logger.LogInformation($"Starting daemon: {crawler_options.Value.DaemonName}");

            while (true)
            {
                if (cancellationTokenSource.IsCancellationRequested) break;

                await Task.Delay(10);
            }

            is_running = false;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"Stopping daemon: {crawler_options.Value.DaemonName}");
            cancellationTokenSource.Cancel();

            while (is_running)
                await Task.Delay(1);
        }

        public void Dispose()
        {
            logger.LogInformation("Disposing....");
        }
    }
}
