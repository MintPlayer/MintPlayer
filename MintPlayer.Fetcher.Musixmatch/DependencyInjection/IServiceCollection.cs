using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Musixmatch;
using MintPlayer.Fetcher.Musixmatch.Parsers;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMusixmatchFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<MusixmatchFetcher>()
                .AddSingleton<IAlbumParser, AlbumParser>()
                .AddSingleton<IArtistParser, ArtistParser>()
                .AddSingleton<ISongParser, SongParser>()
                .AddSingleton<IFetcher, MusixmatchFetcher>();
        }
    }
}
