using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.AZLyrics;
using MintPlayer.Fetcher.AZLyrics.Parsers;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddAZLyricsFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<ISongParser, SongParser>()
                .AddSingleton<IArtistParser, ArtistParser>()
                .AddSingleton<IFetcher, AZLyricsFetcher>();
        }
    }
}
