using System;
using System.Collections.Generic;
using Spa.SpaRoutes.Abstractions;

namespace Spa.SpaRoutes.Data
{
    internal class SpaRouteItem : ISpaRouteItem, ISpaRouteBuilder
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Path { get; set; }
        private readonly List<ISpaRouteItem> Routes = new List<ISpaRouteItem>();

        public ISpaRouteBuilder Route(string path, string name)
        {
            var route = new SpaRouteItem { Path = path, Name = name, FullName = $"{FullName}-{name}" };
            Routes.Add(route);
            return this;
        }

        public ISpaRouteBuilder Group(string path, string name, Action<ISpaRouteBuilder> builder)
        {
            var group = new SpaRouteItem { Path = path, Name = name, FullName = $"{FullName}-{name}" };
            builder(group);
            return this;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
