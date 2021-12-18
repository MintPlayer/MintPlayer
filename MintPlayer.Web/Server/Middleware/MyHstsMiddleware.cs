using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace MintPlayer.Web.Server.Middleware
{
	// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
	public class MyHstsMiddleware
	{
		private const string IncludeSubDomains = "; includeSubDomains";
		private const string Preload = "; preload";

		private readonly RequestDelegate _next;
		private readonly StringValues _strictTransportSecurityValue;
		private readonly IList<string> _excludedHosts;

		/// <summary>
		/// Initialize the HSTS middleware.
		/// </summary>
		/// <param name="next"></param>
		/// <param name="options"></param>
		/// <param name="loggerFactory"></param>
		public MyHstsMiddleware(RequestDelegate next, IOptions<HstsOptions> options, ILoggerFactory loggerFactory)
		{
			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			_next = next ?? throw new ArgumentNullException(nameof(next));

			var hstsOptions = options.Value;
			var maxAge = Convert.ToInt64(Math.Floor(hstsOptions.MaxAge.TotalSeconds))
							.ToString(CultureInfo.InvariantCulture);
			var includeSubdomains = hstsOptions.IncludeSubDomains ? IncludeSubDomains : StringSegment.Empty;
			var preload = hstsOptions.Preload ? Preload : StringSegment.Empty;
			_strictTransportSecurityValue = new StringValues($"max-age={maxAge}{includeSubdomains}{preload}");
			_excludedHosts = hstsOptions.ExcludedHosts;
		}

		/// <summary>
		/// Initialize the HSTS middleware.
		/// </summary>
		/// <param name="next"></param>
		/// <param name="options"></param>
		public MyHstsMiddleware(RequestDelegate next, IOptions<HstsOptions> options)
			: this(next, options, NullLoggerFactory.Instance) { }

		/// <summary>
		/// Invoke the middleware.
		/// </summary>
		/// <param name="context">The <see cref="HttpContext"/>.</param>
		/// <returns></returns>
		public async Task Invoke(HttpContext context)
		{
			//if (!context.Request.IsHttps)
			//{
			//	return _next(context);
			//}

			//if (IsHostExcluded(context.Request.Host.Host))
			//{
			//	return _next(context);
			//}

			context.Response.OnStarting((state) =>
			{
				var httpContext = (HttpContext)state;
				httpContext.Response.Headers.StrictTransportSecurity = _strictTransportSecurityValue;
				return Task.CompletedTask;
			}, context);

			await _next(context);
		}

		//private bool IsHostExcluded(string host)
		//{
		//	for (var i = 0; i < _excludedHosts.Count; i++)
		//	{
		//		if (string.Equals(host, _excludedHosts[i], StringComparison.OrdinalIgnoreCase))
		//		{
		//			return true;
		//		}
		//	}

		//	return false;
		//}
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
