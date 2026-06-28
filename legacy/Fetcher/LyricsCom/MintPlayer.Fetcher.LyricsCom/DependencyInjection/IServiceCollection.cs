using MintPlayer.Fetcher;
using MintPlayer.Fetcher.Abstractions;
using MintPlayer.Fetcher.LyricsCom;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddLyricsComFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<ILyricsComFetcher, LyricsComFetcher>()
                .AddSingleton<IFetcher, LyricsComFetcher>();
        }
    }
}
