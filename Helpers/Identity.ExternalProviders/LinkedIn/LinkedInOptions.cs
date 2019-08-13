using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Identity.ExternalProviders.LinkedIn
{
    public class LinkedInOptions : OAuthOptions
    {
        public LinkedInOptions()
        {
            this.CallbackPath = new PathString("/signin-linkedin");
            this.AuthorizationEndpoint = LinkedInDefaults.AuthorizationEndpoint;
            this.TokenEndpoint = LinkedInDefaults.TokenEndpoint;
            this.UserInformationEndpoint = LinkedInDefaults.UserInformationEndpoint;
            this.Scope.Add("r_basicprofile");
            this.Scope.Add("r_emailaddress");
            this.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id", ClaimValueTypes.String);
            this.ClaimActions.MapJsonKey(ClaimTypes.Name, "formattedName", ClaimValueTypes.String);
            this.ClaimActions.MapJsonKey(ClaimTypes.Email, "emailAddress", ClaimValueTypes.Email);
            this.ClaimActions.MapJsonKey("picture", "pictureUrl", ClaimValueTypes.String);
        }
    }
}
