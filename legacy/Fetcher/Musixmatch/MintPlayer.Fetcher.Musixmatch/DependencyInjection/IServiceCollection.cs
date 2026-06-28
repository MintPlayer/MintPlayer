using MintPlayer.Fetcher;
using MintPlayer.Fetcher.Abstractions;
using MintPlayer.Fetcher.Musixmatch;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMusixmatchFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<IMusixmatchFetcher, MusixmatchFetcher>()
                .AddSingleton<IFetcher, MusixmatchFetcher>();
        }
    }
}
