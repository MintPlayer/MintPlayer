using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.ExternalProviders.LinkedIn
{
    public static class Extensions
    {
        public static AuthenticationBuilder AddLinkedin(this AuthenticationBuilder builder, Action<LinkedInOptions> linkedinOptions)
        {
            return builder.AddOAuth<LinkedInOptions, LinkedinHandler>("LinkedIn", "LinkedIn", linkedinOptions);
        }
    }
}
