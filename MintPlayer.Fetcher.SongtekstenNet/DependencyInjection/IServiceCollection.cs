using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.SongtekstenNet;
using MintPlayer.Fetcher.SongtekstenNet.Parsers;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddSongtekstenNetFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<SongtekstenNetFetcher>()
                .AddSingleton<IArtistParser, ArtistParser>()
                .AddSingleton<IAlbumParser, AlbumParser>()
                .AddSingleton<ISongParser, SongParser>()
                .AddSingleton<IFetcher, SongtekstenNetFetcher>();
        }
    }
}
