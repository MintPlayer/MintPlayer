namespace Identity.ExternalProviders.GitHub
{
    internal static class GitHubDefaults
    {
        internal static readonly string DisplayName = "GitHub";
        internal static readonly string AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        internal static readonly string TokenEndpoint = "https://github.com/login/oauth/access_token";
        internal static readonly string UserInformationEndpoint = "https://api.github.com/user";
        internal const string AuthenticationScheme = "GitHub";
    }
}
