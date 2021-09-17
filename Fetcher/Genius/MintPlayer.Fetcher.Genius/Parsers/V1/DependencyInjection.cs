using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Services;
using MintPlayer.Fetcher.Genius.Parsers.V1.Mappers;
using MintPlayer.Fetcher.Genius.Parsers.V1.Services;

namespace MintPlayer.Fetcher.Genius.Parsers.V1
{
	internal static class DependencyInjection
	{
		internal static IServiceCollection AddV1Parsers(this IServiceCollection services)
		{
			return services
				.AddScoped<IGeniusParser, V1Parser>()
				.AddScoped<IPageDataReader, PageDataReader>()
				.AddV1Parsers()
				.AddV1Mappers();
		}
	}
}
