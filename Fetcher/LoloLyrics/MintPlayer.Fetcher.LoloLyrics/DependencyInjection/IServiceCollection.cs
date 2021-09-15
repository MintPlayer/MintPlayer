using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.LoloLyrics;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddLoloLyricsFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<LoloLyricsFetcher>()
                .AddSingleton<IFetcher, LoloLyricsFetcher>();
        }
    }
}
