using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Options;

namespace MintPlayer.Web.Server.Middleware
{
	// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
	public class MyHstsMiddleware
	{
		private readonly RequestDelegate next;
		private readonly IOptions<HstsOptions> hstsOptions;
		public MyHstsMiddleware(RequestDelegate next, IOptions<Microsoft.AspNetCore.HttpsPolicy.HstsOptions> hstsOptions)
		{
			this.next = next;
			this.hstsOptions = hstsOptions;
		}

		public async Task Invoke(HttpContext httpContext)
		{
			httpContext.Response.OnStarting((state) =>
			{
				var context = (HttpContext)state;

				var hstsValues = new List<string> { $"max-age={hstsOptions.Value.MaxAge.Seconds}" };
				if (hstsOptions.Value.IncludeSubDomains)
				{
					hstsValues.Add("includeSubDomains");
				}

				if (hstsOptions.Value.Preload)
				{
					hstsValues.Add("preload");
				}

				context.Response.Headers.StrictTransportSecurity = new Microsoft.Extensions.Primitives.StringValues(string.Join("; ", hstsValues));

				return Task.CompletedTask;
			}, httpContext);

			await next(httpContext);

		}
	}

	// Extension method used to add the middleware to the HTTP request pipeline.
	public static class MyHstsMiddlewareExtensions
	{
		public static IApplicationBuilder UseMyHsts(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<MyHstsMiddleware>();
		}
	}
}
