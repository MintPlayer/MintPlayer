using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Fetcher.LyricsCom;

namespace MintPlayer.Fetcher.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddLyricsComFetcher(this IServiceCollection services)
        {
            return services
                .AddSingleton<LyricsComFetcher>()
                .AddSingleton<IFetcher, LyricsComFetcher>();
        }
    }
}
