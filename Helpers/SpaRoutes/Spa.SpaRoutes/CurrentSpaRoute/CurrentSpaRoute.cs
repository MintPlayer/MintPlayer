using Microsoft.AspNetCore.Http;
using Spa.SpaRoutes.CurrentSpaRoute.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Spa.SpaRoutes.CurrentSpaRoute
{
    internal class CurrentSpaRoute : ICurrentSpaRoute
    {
        public CurrentSpaRoute(SpaRouteBuilder routeBuilder)
        {
            this.routeBuilder = routeBuilder;
        }

        private SpaRouteBuilder routeBuilder;

        public SpaRoute GetCurrentRoute(HttpContext httpContext)
        {
            var allRoutes = routeBuilder.Build();
            var match = allRoutes.FirstOrDefault(r => IsMatch(httpContext.Request.Path, r.FullPath));

            if (match == null)
                return null;
            else
                return new SpaRoute { Name = match.FullName, Path = match.FullPath, Parameters = new Dictionary<string, object>() };
        }

        private bool IsMatch(string path, string route)
        {
            var rgx = @"\{[a-zA-Z0-9]+\}";
            var replace = "(.*)";
            var formatted_route = Regex.Replace(route, rgx, replace);
            return Regex.IsMatch(path, $"^/{formatted_route}$");
        }
    }
}
