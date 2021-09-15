using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Genius;
using MintPlayer.Fetcher.Genius.Parsers;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddGeniusFetcher(this IServiceCollection services)
        {
			return services
				.AddScoped<GeniusFetcher>()
				.AddScoped<IFetcher, GeniusFetcher>()

				.AddScoped<IGeniusParser, Genius.Parsers.V1.V1Parser>()
				.AddScoped<Genius.Parsers.V1.Album.IAlbumV1Parser, Genius.Parsers.V1.Album.AlbumV1Parser>()
				.AddScoped<Genius.Parsers.V1.Artist.IArtistV1Parser, Genius.Parsers.V1.Artist.ArtistV1Parser>()
				.AddScoped<Genius.Parsers.V1.Song.ISongV1Parser, Genius.Parsers.V1.Song.SongV1Parser>();
        }
    }
}
