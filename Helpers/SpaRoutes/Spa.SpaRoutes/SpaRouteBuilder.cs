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
            routes = new List<ISpaRoute>();
        }

        public List<ISpaRoute> routes { get; private set; }

        public ISpaRouteBuilder Route(string path, string name)
        {
            var route = new SpaRoute { Path = path, Name = name };
            routes.Add(route);
            return this;
        }

        public ISpaRouteBuilder Group(string path, string name, Action<ISpaRouteBuilder> builder)
        {
            var group = new SpaRoute { Path = path, Name = name };
            builder(group);
            routes.Add(group);
            return this;
        }

        public ISpaRouteCollection Build()
        {
            var routeCollection = new SpaRouteCollection();
            foreach (var route in routes)
            {
                routeCollection.Add(route);
            }
            return routeCollection;
        }
    }
}
