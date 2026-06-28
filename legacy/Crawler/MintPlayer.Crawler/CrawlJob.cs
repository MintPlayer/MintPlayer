using System;
using MintPlayer.Crawler.Enums;

namespace MintPlayer.Crawler
{
    public class CrawlJob<TResult>
    {
        public CrawlJob()
        {
            Status = CrawlStatus.Pending;
        }

        public string Url { get; set; }
        public CrawlStatus Status { get; set; }
        public Exception Exception { get; set; }
        public TResult Result { get; set; }
    }
}
