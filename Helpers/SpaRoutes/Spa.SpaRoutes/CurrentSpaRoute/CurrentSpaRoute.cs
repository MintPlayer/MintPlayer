using Microsoft.AspNetCore.Http;
using Spa.SpaRoutes.CurrentSpaRoute.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Spa.SpaRoutes.CurrentSpaRoute
{
    internal class CurrentSpaRoute : ICurrentSpaRoute
    {
        public SpaRoute GetCurrentRoute(HttpContext httpContext)
        {
            throw new NotImplementedException();
        }
    }
}
