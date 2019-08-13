using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.ExternalProviders.Pinterest
{
    public static class Extensions
    {
        public static AuthenticationBuilder AddPinterest(this AuthenticationBuilder builder, Action<PinterestOptions> pinterestOptions)
        {
            return builder.AddOAuth<PinterestOptions, PinterestHandler>("Pinterest", "Pinterest", pinterestOptions);
        }
    }
}
