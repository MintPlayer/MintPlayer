using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V2.Parsers;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Parsers
{
	internal static class DependencyInjection
	{
		public static IServiceCollection AddV2Parsers(this IServiceCollection services)
		{
			return services
				.AddScoped<IArtistV2Parser, ArtistV2Parser>()
				.AddScoped<ISongV2Parser, SongV2Parser>()
				.AddScoped<IAlbumV2Parser, AlbumV2Parser>();
		}
	}
}
