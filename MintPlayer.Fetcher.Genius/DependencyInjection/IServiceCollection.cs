using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Genius;
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
                .AddSingleton<Genius.Parsers.IGeniusParser, Genius.Parsers.GeniusParser>()
                .AddSingleton<Genius.Parsers.IGeniusVersionParser, Genius.Parsers.V1.V1Parser>()
                .AddSingleton<Genius.Parsers.IGeniusVersionParser, Genius.Parsers.V2.V2Parser>()
                .AddSingleton<Genius.Parsers.IGeniusVersionParser, Genius.Parsers.V3.V3Parser>()
                .AddSingleton<Genius.Parsers.V1.Album.IAlbumParser, Genius.Parsers.V1.Album.AlbumParser>()
                .AddSingleton<Genius.Parsers.V1.Artist.IArtistParser, Genius.Parsers.V1.Artist.ArtistParser>()
                .AddSingleton<Genius.Parsers.V1.Song.ISongParser, Genius.Parsers.V1.Song.SongParser>()
                .AddSingleton<Genius.Parsers.V1.Helpers.IPageDataReader, Genius.Parsers.V1.Helpers.PageDataReader>()
                .AddSingleton<Genius.Parsers.V2.Album.IAlbumParser, Genius.Parsers.V2.Album.AlbumParser>()
                .AddSingleton<Genius.Parsers.V2.Artist.IArtistParser, Genius.Parsers.V2.Artist.ArtistParser>()
                .AddSingleton<Genius.Parsers.V2.Song.ISongParser, Genius.Parsers.V2.Song.SongParser>()
                .AddSingleton<Genius.Parsers.V3.Album.IAlbumParser, Genius.Parsers.V3.Album.AlbumParser>()
                .AddSingleton<Genius.Parsers.V3.Artist.IArtistParser, Genius.Parsers.V3.Artist.ArtistParser>()
                .AddSingleton<Genius.Parsers.V3.Song.ISongParser, Genius.Parsers.V3.Song.SongParser>()
                .AddSingleton<IFetcher, GeniusFetcher>();
        }
    }
}
