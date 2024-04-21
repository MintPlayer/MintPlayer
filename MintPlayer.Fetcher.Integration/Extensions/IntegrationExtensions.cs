using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.Integration.Services;

namespace MintPlayer.Fetcher.Integration.Extensions;

public static class IntegrationExtensions
{
	public static IServiceCollection AddFetcherIntegration(this IServiceCollection services)
		=> services.AddScoped<IFetcherService, FetcherService>();
}
