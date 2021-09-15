using Microsoft.Extensions.DependencyInjection;

namespace MintPlayer.Fetcher.Genius.Parsers.V2
{
	internal static class DependencyInjection
	{
		internal static IServiceCollection AddV2Parsers(this IServiceCollection services)
		{
			return services
				.AddScoped<IGeniusParser, V2Parser>()
				.AddScoped<Artist.IArtistV2Parser, Artist.ArtistV2Parser>()
				.AddScoped<Song.ISongV2Parser, Song.SongV2Parser>()
				.AddScoped<Album.IAlbumV2Parser, Album.AlbumV2Parser>()
				.AddScoped<Services.IPreloadedStateReader, Services.PreloadedStateReader>();
		}
	}
}
