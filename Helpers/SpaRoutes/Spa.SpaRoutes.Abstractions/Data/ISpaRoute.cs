using System.Collections.Generic;

namespace Spa.SpaRoutes.Abstractions
{
    public interface ISpaRoute
    {
        string Name { get; set; }
        string Path { get; set; }
    }
}
