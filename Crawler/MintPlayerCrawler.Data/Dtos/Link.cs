namespace MintPlayerCrawler.Data.Dtos
{
    public class Link
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public Link Via { get; set; }
    }
}
