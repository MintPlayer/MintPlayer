using System;
using System.Collections.Generic;
using System.Linq;
using Spa.SpaRoutes.Abstractions;
using Spa.SpaRoutes.Data;

namespace Spa.SpaRoutes
{
    public class SpaRouteBuilder : ISpaRouteBuilder
    {
        public SpaRouteBuilder()
        {
            routes = new List<ISpaRouteItem>();
        }

        public List<ISpaRouteItem> routes { get; private set; }

        public ISpaRouteBuilder Route(string path, string name)
        {
            var route = new SpaRouteItem { Path = path, Name = name, FullName = name };
            routes.Add(route);
            return this;
        }

        public ISpaRouteBuilder Group(string path, string name, Action<ISpaRouteBuilder> builder)
        {
            var group = new SpaRouteItem { Path = path, Name = name, FullName = name };
            builder(group);
            routes.Add(group);
            return this;
        }
    }
}
