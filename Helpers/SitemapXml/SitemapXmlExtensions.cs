using Microsoft.Extensions.DependencyInjection;
using SitemapXml.Options;
using System;

namespace SitemapXml
{
    public static class SitemapXmlExtensions
    {
        public static IServiceCollection AddSitemapXml(this IServiceCollection services)
        {
            services.AddScoped<DependencyInjection.Interfaces.ISitemapXml, DependencyInjection.SitemapXml>();
            return services;
        }

        public static IMvcBuilder AddSitemapXmlFormatters(this IMvcBuilder mvc, Action<SitemapXmlOptions> options)
        {
            var opt = new SitemapXmlOptions();
            options(opt);

            mvc.AddMvcOptions(mvc_options =>
            {
                if (!string.IsNullOrEmpty(opt.Stylesheet))
                    mvc_options.OutputFormatters.Insert(0, new Formatters.XmlSerializerOutputFormatter(opt.Stylesheet));
            });
            return mvc;
        }
    }
}
