﻿using System;
using System.Collections.Generic;

namespace Spa.SpaRoutes.Abstractions
{
    public interface ISpaRouteBuilder
    {
        ISpaRouteBuilder Route(string path, string name);
        ISpaRouteBuilder Group(string path, string name, Action<ISpaRouteBuilder> builder);
        ISpaRouteCollection Build();
    }
}
