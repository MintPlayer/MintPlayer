using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Genius.Parsers.V1.Mappers;
using MintPlayer.Fetcher.Genius.Parsers.V1.Parsers;
using MintPlayer.Fetcher.Genius.Parsers.V1.Services;

namespace MintPlayer.Fetcher.Genius.Parsers.V1
{
	internal static class DependencyInjection
	{
		internal static IServiceCollection AddV1Parser(this IServiceCollection services)
		{
			return services
				.AddScoped<IGeniusParser, V1Parser>()
				.AddV1Mappers()
				.AddV1Services()
				.AddV1Parsers();
		}
	}
}
