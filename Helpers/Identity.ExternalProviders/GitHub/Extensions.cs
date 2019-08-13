using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.ExternalProviders.GitHub
{
    public static class Extensions
    {
        public static AuthenticationBuilder AddGitHub(this AuthenticationBuilder builder, Action<GitHubOptions> githubOptions)
        {
            return builder.AddOAuth<GitHubOptions, GitHubHandler>("GitHub", "GitHub", githubOptions);
        }
    }
}
