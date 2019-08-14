using System;
using System.Collections.Generic;
using Spa.SpaRoutes.Abstractions;

namespace Spa.SpaRoutes.Data
{
    public class SpaRoute : ISpaRoute, ISpaRouteBuilder
    {
        public string Name { get; set; }
        public string Path { get; set; }
        private readonly List<ISpaRoute> Routes = new List<ISpaRoute>();

        public ISpaRouteBuilder Route(string path, string name)
        {
            var route = new SpaRoute { Path = path, Name = name };
            Routes.Add(route);
            return this;
        }

        public ISpaRouteBuilder Group(string path, string name, Action<ISpaRouteBuilder> builder)
        {
            var group = new SpaRoute { Path = path, Name = name };
            builder(group);
            return this;
        }

        public ISpaRouteCollection Build()
        {
            throw new NotImplementedException();
        }


        //public static ISpaRoute FromDelegate(Action<ISpaRoute> action)
        //{
        //    var route = new SpaRoute();
        //    action(route);
        //    return route;
        //}
    }
}
