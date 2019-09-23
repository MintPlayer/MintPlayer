namespace MintPlayerCrawler.Data.Entities.Jobs
{
    internal class IndexJob : Job
    {
        public string Url { get; set; }
        public string Html { get; set; }
    }
}
