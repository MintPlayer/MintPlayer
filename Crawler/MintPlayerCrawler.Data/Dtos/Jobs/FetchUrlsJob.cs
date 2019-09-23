namespace MintPlayerCrawler.Data.Dtos.Jobs
{
    public class FetchUrlsJob : Job
    {
        public string Url { get; set; }
        public string Html { get; set; }
    }
}
