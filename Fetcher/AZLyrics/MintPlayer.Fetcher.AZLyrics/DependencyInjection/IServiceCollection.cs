using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.AZLyrics;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddAZLyricsFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<AZLyricsFetcher>()
                .AddSingleton<IFetcher, AZLyricsFetcher>();
        }
    }
}
