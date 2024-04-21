using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Parsers;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Parsers;

internal static class DependencyInjection
{
	public static IServiceCollection AddV1Parsers(this IServiceCollection services)
	{
		return services
			.AddScoped<IArtistV1Parser, ArtistV1Parser>()
			.AddScoped<ISongV1Parser, SongV1Parser>()
			.AddScoped<IAlbumV1Parser, AlbumV1Parser>();
	}
}
