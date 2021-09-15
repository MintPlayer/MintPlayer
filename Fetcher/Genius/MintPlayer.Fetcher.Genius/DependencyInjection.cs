using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Genius;
using MintPlayer.Fetcher.Genius.Parsers;

namespace MintPlayer.Fetcher.DependencyInjection
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddGeniusFetcher(this IServiceCollection services)
		{
			return services
				.AddScoped<GeniusFetcher>()
				.AddScoped<IFetcher, GeniusFetcher>()
				.AddParsers();
		}
	}
}
