using MintPlayer.Fetcher;
using MintPlayer.Fetcher.Abstractions;
using MintPlayer.Fetcher.Muzikum;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddMuzikumFetcher(this IServiceCollection services)
		{
			return services
				.AddScoped<IMuzikumFetcher, MuzikumFetcher>()
				.AddScoped<IFetcher, MuzikumFetcher>();
		}
	}
}
