using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Musixmatch;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMusixmatchFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<MusixmatchFetcher>()
                .AddSingleton<IFetcher, MusixmatchFetcher>();
        }
    }
}
