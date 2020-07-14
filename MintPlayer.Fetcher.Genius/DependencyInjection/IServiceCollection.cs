using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Genius;
using MintPlayer.Fetcher.Genius.Parsers;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MintPlayer.Fetcher.Genius.Test")]
namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddGeniusFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<GeniusFetcher>()
                .AddSingleton<IV1Parser, V1Parser>()
                .AddSingleton<IV2Parser, V2Parser>()
                .AddSingleton<IV3Parser, V3Parser>()
                .AddSingleton<Genius.Parsers.V1.IAlbumParser, Genius.Parsers.V1.AlbumParser>()
                .AddSingleton<Genius.Parsers.V1.IArtistParser, Genius.Parsers.V1.ArtistParser>()
                .AddSingleton<Genius.Parsers.V1.ISongParser, Genius.Parsers.V1.SongParser>()
                .AddSingleton<Genius.Parsers.V2.IAlbumParser, Genius.Parsers.V2.AlbumParser>()
                .AddSingleton<Genius.Parsers.V2.IArtistParser, Genius.Parsers.V2.ArtistParser>()
                .AddSingleton<Genius.Parsers.V2.ISongParser, Genius.Parsers.V2.SongParser>()
                .AddSingleton<IFetcher, GeniusFetcher>();
        }
    }
}
