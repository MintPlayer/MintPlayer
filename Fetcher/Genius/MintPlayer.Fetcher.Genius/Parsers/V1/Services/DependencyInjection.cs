using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Genius.Abstractions.Parsers.V1.Services;

namespace MintPlayer.Fetcher.Genius.Parsers.V1.Services
{
	internal static class DependencyInjection
	{
		public static IServiceCollection AddV1Services(this IServiceCollection services)
		{
			return services
				.AddScoped<IPageDataReader, PageDataReader>();
		}
	}
}
