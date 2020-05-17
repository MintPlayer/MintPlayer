using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.SongLyrics;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddSongLyricsFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<SongLyricsFetcher>()
                .AddSingleton<IFetcher, SongLyricsFetcher>();
        }
    }
}
