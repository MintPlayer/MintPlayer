using MintPlayer.Data.Options;

namespace MintPlayerCrawler.Data.Options
{
    public class MintPlayerCrawlerSettings
    {
        public string MintPlayerConnectionString { get; set; }
        public string ConnectionString { get; set; }
        public JwtIssuerOptions JwtIssuerOptions { get; set; }
    }
}
