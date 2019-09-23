namespace MintPlayerCrawler.Data.Entities.Jobs
{
    internal class FetchUrlsJob : Job
    {
        public string Url { get; set; }
        public string Html { get; set; }
    }
}
