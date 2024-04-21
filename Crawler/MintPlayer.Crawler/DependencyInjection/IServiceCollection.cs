using Microsoft.Extensions.DependencyInjection;

namespace MintPlayer.Crawler.DependencyInjection;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddMintPlayerCrawler(this IServiceCollection services)
	{
		return services.AddSingleton<MintPlayerCrawler>();
	}
}
