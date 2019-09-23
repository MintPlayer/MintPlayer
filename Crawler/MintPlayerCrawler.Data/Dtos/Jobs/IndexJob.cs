namespace MintPlayerCrawler.Data.Dtos.Jobs
{
    public class IndexJob : Job
    {
        public string Url { get; set; }
        public string Html { get; set; }
    }
}
