using MintPlayer.Fetcher;
using MintPlayer.Fetcher.Abstractions;
using MintPlayer.Fetcher.AZLyrics;
using MintPlayer.Fetcher.AZLyrics.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddAZLyricsFetcher(this IServiceCollection services)
	{
		return services
			.AddSingleton<IAZLyricsFetcher, AZLyricsFetcher>()
			.AddSingleton<IFetcher, AZLyricsFetcher>();
	}
}
