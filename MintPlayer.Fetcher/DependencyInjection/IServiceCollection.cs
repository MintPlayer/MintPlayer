using Microsoft.Extensions.DependencyInjection;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddFetcherContainer(this IServiceCollection services)
        {
            return services.AddSingleton<IFetcherContainer, FetcherContainer>();
        }
    }
}
