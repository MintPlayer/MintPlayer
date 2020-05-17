using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Genius;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddGeniusFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<GeniusFetcher>()
                .AddSingleton<IFetcher, GeniusFetcher>();
        }
    }
}
