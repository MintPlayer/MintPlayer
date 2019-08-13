using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Identity.ExternalProviders.Pinterest
{
    public class PinterestOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PinterestOptions"/> class.
        /// </summary>
        public PinterestOptions()
        {
            this.CallbackPath = new PathString("/signin-pinterest");
            this.AuthorizationEndpoint = PinterestDefaults.AuthorizationEndpoint;
            this.TokenEndpoint = PinterestDefaults.TokenEndpoint;

            this.UserInformationEndpoint = PinterestDefaults.UserInformationEndpoint;
            this.Scope.Add("user");

            this.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id", ClaimValueTypes.String);
            this.ClaimActions.MapJsonKey(ClaimTypes.Name, "login", ClaimValueTypes.String);
            this.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.String);
            this.ClaimActions.MapJsonKey(ClaimTypes.Webpage, "html_url", ClaimValueTypes.String);
        }
    }
}
