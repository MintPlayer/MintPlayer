using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using MintPlayer.RestClient.Options;

namespace MintPlayer.RestClient.DependencyInjection
{
    public static class RestClientExtensions
    {
        public static IServiceCollection AddRestClient(this IServiceCollection services, Action<RestClientOptions> options)
        {
            return services
                .AddSingleton<HttpClient>()
                .AddSingleton<IRestClient, RestClient>()
                .Configure<RestClientOptions>(options);
        }
    }
}
