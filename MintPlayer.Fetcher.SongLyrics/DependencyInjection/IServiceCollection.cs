using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.SongLyrics;
using MintPlayer.Fetcher.SongLyrics.Parsers;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddSongLyricsFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<IArtistParser, ArtistParser>()
                .AddSingleton<IAlbumParser, AlbumParser>()
                .AddSingleton<ISongParser, SongParser>()
                .AddSingleton<SongLyricsFetcher>()
                .AddSingleton<IFetcher, SongLyricsFetcher>();
        }
    }
}
