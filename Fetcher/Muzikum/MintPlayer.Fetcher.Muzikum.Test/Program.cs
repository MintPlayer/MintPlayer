using Microsoft.Extensions.DependencyInjection;

namespace MintPlayer.Fetcher.Muzikum.Test;

class Program
{
	static void Main(string[] args)
	{
		var services = new ServiceCollection()
			.AddSingleton<HttpClient>()
			.AddFetcherContainer()
			.BuildServiceProvider();
	}
}
