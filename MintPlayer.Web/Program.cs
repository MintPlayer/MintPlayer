namespace MintPlayer.Web;

public class Program
{
	public static async Task Main(string[] args)
	{
		await CreateHostBuilder(args).Build().RunAsync();
	}

	public static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder
					.UseStartup<Startup>()
					.UseUrls(
						"https://localhost:44329",
						"https://mintplayer.test:44329",
						"https://external.mintplayer.test:44329",
						"https://mintplayer.com",
						"https://external.mintplayer.com"
					)
					.UseIISIntegration();
			});
}
