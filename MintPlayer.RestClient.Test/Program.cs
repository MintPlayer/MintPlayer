using Microsoft.Extensions.DependencyInjection;
using MintPlayer.RestClient.DependencyInjection;
using System.Threading.Tasks;

namespace MintPlayer.RestClient.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddRestClient(options =>
                {
                    options.BaseUrl = "https://mintplayer.com/api";
                });

            var provider = services.BuildServiceProvider();
            var client = provider.GetService<IRestClient>();

            client.AdditionalHeaders.Add("Test", "Pieterjan");
            await client.Get<Dtos.Dtos.Artist>(
                "/Artist/8",
                (response) =>
                {

                },
                () =>
                {
                }
            );
        }
    }


}
