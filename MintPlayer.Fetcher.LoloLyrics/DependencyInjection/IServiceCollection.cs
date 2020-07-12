using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.LoloLyrics;
using MintPlayer.Fetcher.LoloLyrics.Parsers;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddLoloLyricsFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<LoloLyricsFetcher>()
                .AddSingleton<ISongParser, SongParser>()
                .AddSingleton<IArtistParser, ArtistParser>()
                .AddSingleton<IFetcher, LoloLyricsFetcher>();
        }
    }
}
