using Microsoft.Extensions.DependencyInjection;

namespace MintPlayer.Fetcher.Genius.Parsers.V1
{
	internal static class DependencyInjection
	{
		internal static IServiceCollection AddV1Parsers(this IServiceCollection services)
		{
			return services
				.AddScoped<IGeniusParser, V1Parser>()
				.AddScoped<Artist.IArtistV1Parser, Artist.ArtistV1Parser>()
				.AddScoped<Song.ISongV1Parser, Song.SongV1Parser>()
				.AddScoped<Album.IAlbumV1Parser, Album.AlbumV1Parser>()
				.AddScoped<Services.IPageDataReader, Services.PageDataReader>();
		}
	}
}
