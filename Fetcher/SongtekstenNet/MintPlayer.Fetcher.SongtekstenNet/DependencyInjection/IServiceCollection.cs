using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.SongtekstenNet;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddSongtekstenNetFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<SongtekstenNetFetcher>()
                .AddSingleton<IFetcher, SongtekstenNetFetcher>();
        }
    }
}
