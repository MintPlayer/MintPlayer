using System;
using System.Collections.Generic;
using Spa.SpaRoutes.Abstractions;

namespace Spa.SpaRoutes.Data
{
    public class SpaRouteCollection : ISpaRouteCollection
    {
        public SpaRouteCollection()
        {
            routes = new List<ISpaRoute>();
        }

        public string Name { get; set; }
        public string Path { get; set; }

        private readonly List<ISpaRoute> routes = new List<ISpaRoute>();

        public void Add(ISpaRoute route)
        {
            if (route == null)
                throw new ArgumentNullException(nameof(route));

            routes.Add(route);
        }
    }
}
