using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.SongMeanings;
using MintPlayer.Fetcher.SongMeanings.Parsers;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddSongMeaningsFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<SongMeaningsFetcher>()
                .AddSingleton<IArtistParser, ArtistParser>()
                .AddSingleton<IAlbumParser, AlbumParser>()
                .AddSingleton<ISongParser, SongParser>()
                .AddSingleton<IFetcher, SongMeaningsFetcher>();
        }
    }
}
