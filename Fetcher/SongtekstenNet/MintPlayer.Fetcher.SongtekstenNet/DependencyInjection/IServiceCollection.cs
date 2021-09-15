using MintPlayer.Fetcher;
using MintPlayer.Fetcher.Abstractions;
using MintPlayer.Fetcher.SongtekstenNet;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddSongtekstenNetFetcher(this IServiceCollection services)
        {
            return services
				.AddSingleton<ISongtekstenNetFetcher, SongtekstenNetFetcher>()
				.AddSingleton<IFetcher, SongtekstenNetFetcher>();
        }
    }
}
