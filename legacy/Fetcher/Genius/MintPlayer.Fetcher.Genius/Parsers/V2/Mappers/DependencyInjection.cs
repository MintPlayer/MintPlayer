using Microsoft.Extensions.DependencyInjection;

namespace MintPlayer.Fetcher.Genius.Parsers.V2.Mappers
{
	internal static class DependencyInjection
	{
		public static IServiceCollection AddV2Mappers(this IServiceCollection services)
		{
			return services
				.AddScoped<SongV2Mapper>();
		}
	}
}
