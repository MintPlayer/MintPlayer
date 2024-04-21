using MintPlayer.Fetcher;
using MintPlayer.Fetcher.Abstractions;
using MintPlayer.Fetcher.SongMeanings;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddSongMeaningsFetcher(this IServiceCollection services)
	{
		return services
			.AddSingleton<ISongMeaningsFetcher, SongMeaningsFetcher>()
			.AddSingleton<IFetcher, SongMeaningsFetcher>();
	}
}
