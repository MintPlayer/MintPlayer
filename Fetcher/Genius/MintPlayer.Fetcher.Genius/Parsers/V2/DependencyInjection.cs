using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V2.Parsers;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V2.Services;
using MintPlayer.Fetcher.Genius.Parsers.V2.Mappers;
using MintPlayer.Fetcher.Genius.Parsers.V2.Parsers;

namespace MintPlayer.Fetcher.Genius.Parsers.V2
{
	internal static class DependencyInjection
	{
		internal static IServiceCollection AddV2Parser(this IServiceCollection services)
		{
			return services
				.AddV2Parsers()
				.AddV2Mappers()
				.AddScoped<IGeniusParser, V2Parser>()
				.AddScoped<IPreloadedStateReader, Services.PreloadedStateReader>();
		}
	}
}
