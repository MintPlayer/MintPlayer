using System;
using Microsoft.AspNetCore.SpaServices;
using Spa.SpaRoutes.Abstractions;

namespace Spa.SpaRoutes
{
    public static class SpaRouteExtensions
    {
        public static ISpaBuilder DefineSpaRoutes(this ISpaBuilder spa, Action<ISpaRouteBuilder> builder)
        {
            var routes = new SpaRouteBuilder();
            builder(routes);
            return spa;
        }
    }
}
