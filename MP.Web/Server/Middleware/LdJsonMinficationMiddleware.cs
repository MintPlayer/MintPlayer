using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Web.Server.Middleware
{
    public class LdJsonMinficationMiddleware
    {
        private readonly RequestDelegate next;
        public LdJsonMinficationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
		{
			var stream = context.Response.Body;
			using (var buffer = new MemoryStream())
			{
				context.Response.Body = buffer;
				await next(context);

				buffer.Seek(0, SeekOrigin.Begin);
				using (var reader = new StreamReader(buffer))
				{
					string responseBody = await reader.ReadToEndAsync();
					var isHtml = context.Response.ContentType?.ToLower().Contains("text/html");
					if (context.Response.StatusCode == 200 && isHtml.GetValueOrDefault())
					{
						responseBody = Regex.Replace(responseBody, @">\s+<", "><", RegexOptions.Compiled);

						using (var memoryStream = new MemoryStream())
						{
							var bytes = System.Text.Encoding.UTF8.GetBytes(responseBody);
							memoryStream.Write(bytes, 0, bytes.Length);
							memoryStream.Seek(0, SeekOrigin.Begin);
							await memoryStream.CopyToAsync(stream, bytes.Length);
						}
					}
				}
			}
		}
    }
}