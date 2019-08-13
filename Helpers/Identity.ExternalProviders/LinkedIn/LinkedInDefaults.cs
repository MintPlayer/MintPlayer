namespace Identity.ExternalProviders.LinkedIn
{
    public static class LinkedInDefaults
    {
        public static readonly string DisplayName = "LinkedIn";
        public static readonly string AuthorizationEndpoint = "https://www.linkedin.com/oauth/v2/authorization";
        public static readonly string TokenEndpoint = "https://www.linkedin.com/oauth/v2/accessToken";
        public static readonly string UserInformationEndpoint = "https://api.linkedin.com/v1/people/~:(id,formatted-name,email-address,picture-url)";
        public const string AuthenticationScheme = "LinkedIn";
    }
}
