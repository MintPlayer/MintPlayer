using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Options;

namespace OpenSearch.Extensions
{
    public static class OpenSearchExtensions
    {
        public static IApplicationBuilder UseOpenSearch(this IApplicationBuilder app, Action<OpenSearchOptions> options)
        {
            var opt = new OpenSearchOptions();
            options(opt);

            if (!opt.OsdxEndpoint.StartsWith('/'))
                throw new Exception(@"OpenSearch endpoint must start with ""/""");

            return app.Use(async (context, next) =>
            {
                if (context.Request.Path == opt.OsdxEndpoint)
                {
                    context.Response.ContentType = "application/opensearchdescription+xml; charset=utf-8";
                    context.Response.Headers["Content-Disposition"] = "attachment; filename=opensearch.osdx";
                    await context.WriteModelAsync(new Data.OpenSearchDescription
                    {
                        ShortName = "MintPlayer",
                        Description = "Search music on MintPlayer",
                        InputEncoding = "UTF-8",
                        Image = new Data.Image
                        {
                            Width = 16,
                            Height = 16,
                            Url = $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.PathBase}{opt.ImageUrl}",
                            Type = "image/png"
                        },
                        Urls = (new[] {
                            new Data.Url { Type = "text/html", Method = "GET", Template = $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.PathBase}{opt.SearchUrl}" },
                            new Data.Url { Type = "application/x-suggestions+json", Method = "GET", Template = $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.PathBase}{opt.SuggestUrl}" },
                            new Data.Url { Type = "application/opensearchdescription+xml", Relation = "self", Template = $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.PathBase}{opt.OsdxEndpoint}" }
                        }).ToList(),
                        Contact = "pieterjandeclippel@msn.com"
                        //SearchForm = $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.PathBase}/"
                    });
                }
                else
                {
                    await next();
                }
            });
        }
    }
}
