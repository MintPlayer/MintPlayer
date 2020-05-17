using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.SongMeanings;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddSongMeaningsFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<SongMeaningsFetcher>()
                .AddSingleton<IFetcher, SongMeaningsFetcher>();
        }
    }
}
