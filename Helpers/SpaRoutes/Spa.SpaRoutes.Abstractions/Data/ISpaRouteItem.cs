using System.Collections.Generic;

namespace Spa.SpaRoutes.Abstractions
{
    public interface ISpaRouteItem
    {
        string Name { get; set; }
        string Path { get; set; }
    }
}
