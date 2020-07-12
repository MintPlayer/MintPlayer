using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.LyricsCom;
using MintPlayer.Fetcher.LyricsCom.Parsers;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddLyricsComFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<LyricsComFetcher>()
                .AddSingleton<IArtistParser, ArtistParser>()
                .AddSingleton<IAlbumParser, AlbumParser>()
                .AddSingleton<ISongParser, SongParser>()
                .AddSingleton<IFetcher, LyricsComFetcher>();
        }
    }
}
