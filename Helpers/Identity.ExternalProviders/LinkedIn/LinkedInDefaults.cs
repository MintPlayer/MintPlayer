namespace Identity.ExternalProviders.LinkedIn
{
    internal static class LinkedInDefaults
    {
        internal static readonly string DisplayName = "LinkedIn";
        internal static readonly string AuthorizationEndpoint = "https://www.linkedin.com/oauth/v2/authorization";
        internal static readonly string TokenEndpoint = "https://www.linkedin.com/oauth/v2/accessToken";
        internal static readonly string UserInformationEndpoint = "https://api.linkedin.com/v1/people/~:(id,formatted-name,email-address,picture-url)";
        internal const string AuthenticationScheme = "LinkedIn";
    }
}
