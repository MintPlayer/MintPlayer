using MintPlayer.Crawler.Enums;

namespace MintPlayer.Crawler;

public class CrawlJob<TResult>
{
	public string Url { get; init; }
	public ECrawlStatus Status { get; set; } = ECrawlStatus.Pending;
	public Exception Exception { get; set; }
	public TResult Result { get; set; }
}
