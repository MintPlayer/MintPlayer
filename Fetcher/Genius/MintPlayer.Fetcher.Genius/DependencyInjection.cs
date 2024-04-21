using MintPlayer.Fetcher;
using MintPlayer.Fetcher.Abstractions;
using MintPlayer.Fetcher.Genius;
using MintPlayer.Fetcher.Genius.Abstractions;
using MintPlayer.Fetcher.Genius.Parsers;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
	public static IServiceCollection AddGeniusFetcher(this IServiceCollection services)
	{
		return services
			.AddScoped<IGeniusFetcher, GeniusFetcher>()
			.AddScoped<IFetcher, GeniusFetcher>()
			.AddParsers();
	}
}
