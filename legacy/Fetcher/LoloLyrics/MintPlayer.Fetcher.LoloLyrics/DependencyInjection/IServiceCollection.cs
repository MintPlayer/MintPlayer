using MintPlayer.Fetcher;
using MintPlayer.Fetcher.Abstractions;
using MintPlayer.Fetcher.LoloLyrics;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddLoloLyricsFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<ILoloLyricsFetcher, LoloLyricsFetcher>()
                .AddSingleton<IFetcher, LoloLyricsFetcher>();
        }
    }
}
