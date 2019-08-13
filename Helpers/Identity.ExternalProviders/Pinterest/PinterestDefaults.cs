namespace Identity.ExternalProviders.Pinterest
{
    internal static class PinterestDefaults
    {
        internal static readonly string DisplayName = "Pinterest";
        internal static readonly string AuthorizationEndpoint = "https://api.pinterest.com/oauth/";
        internal static readonly string TokenEndpoint = "https://api.pinterest.com/v1/oauth/token";
        internal static readonly string UserInformationEndpoint = "https://api.pinterest.com/v1/me";
        internal const string AuthenticationScheme = "Pinterest";
    }
}
