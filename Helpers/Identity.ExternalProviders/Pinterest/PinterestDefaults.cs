namespace Identity.ExternalProviders.Pinterest
{
    public static class PinterestDefaults
    {
        public static readonly string DisplayName = "Pinterest";
        public static readonly string AuthorizationEndpoint = "https://api.pinterest.com/oauth/";
        public static readonly string TokenEndpoint = "https://api.pinterest.com/v1/oauth/token";
        public static readonly string UserInformationEndpoint = "https://api.pinterest.com/v1/me";
        public const string AuthenticationScheme = "Pinterest";
    }
}
