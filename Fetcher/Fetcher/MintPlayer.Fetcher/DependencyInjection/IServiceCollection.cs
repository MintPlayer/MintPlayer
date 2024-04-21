using MintPlayer.Fetcher;
using MintPlayer.Fetcher.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddFetcherContainer(this IServiceCollection services)
	{
		return services.AddSingleton<IFetcherContainer, FetcherContainer>();
	}
}
