using MintPlayer.Fetcher;
using MintPlayer.Fetcher.Abstractions;
using MintPlayer.Fetcher.SongLyrics;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddSongLyricsFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<ISongLyricsFetcher, SongLyricsFetcher>()
                .AddSingleton<IFetcher, SongLyricsFetcher>();
        }
    }
}
