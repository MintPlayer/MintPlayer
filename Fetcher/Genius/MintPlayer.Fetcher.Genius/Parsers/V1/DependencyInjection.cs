using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Album;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Artist;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Services;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Song;
using MintPlayer.Fetcher.Genius.Parsers.V1.Services;

namespace MintPlayer.Fetcher.Genius.Parsers.V1
{
	internal static class DependencyInjection
	{
		internal static IServiceCollection AddV1Parsers(this IServiceCollection services)
		{
			return services
				.AddScoped<IGeniusParser, V1Parser>()
				.AddScoped<IArtistV1Parser, Artist.ArtistV1Parser>()
				.AddScoped<ISongV1Parser, Song.SongV1Parser>()
				.AddScoped<IAlbumV1Parser, Album.AlbumV1Parser>()
				.AddScoped<IPageDataReader, PageDataReader>();
		}
	}
}
