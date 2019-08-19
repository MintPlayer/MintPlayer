using Microsoft.Extensions.DependencyInjection;

namespace SitemapXml
{
    public static class SitemapXmlExtensions
    {
        public static IServiceCollection AddSitemapXml(this IServiceCollection services)
        {
            services.AddScoped<SitemapXml.Interfaces.ISitemapXml, SitemapXml.SitemapXml>();
            return services;
        }
    }
}
