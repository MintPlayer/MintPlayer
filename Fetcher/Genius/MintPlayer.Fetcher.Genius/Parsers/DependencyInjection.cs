using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Genius.Parsers.V1;
using MintPlayer.Fetcher.Genius.Parsers.V2;

namespace MintPlayer.Fetcher.Genius.Parsers
{
	internal static class DependencyInjection
	{
		internal static IServiceCollection AddParsers(this IServiceCollection services)
		{
			return services
				.AddV1Parser()
				.AddV2Parser()
				.AddScoped<Abstractions.Parsers.Helpers.ILyricsParser, Helpers.LyricsParser>();
		}
	}
}
