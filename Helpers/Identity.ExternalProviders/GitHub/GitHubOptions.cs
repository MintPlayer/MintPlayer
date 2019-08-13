using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Identity.ExternalProviders.GitHub
{
    public class GitHubOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubOptions"/> class.
        /// </summary>
        public GitHubOptions()
        {
            this.CallbackPath = new PathString("/signin-github");
            this.AuthorizationEndpoint = GitHubDefaults.AuthorizationEndpoint;
            this.TokenEndpoint = GitHubDefaults.TokenEndpoint;

            this.UserInformationEndpoint = GitHubDefaults.UserInformationEndpoint;
            this.Scope.Add("user");

            this.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id", ClaimValueTypes.String);
            this.ClaimActions.MapJsonKey(ClaimTypes.Name, "login", ClaimValueTypes.String);
            this.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.String);
            this.ClaimActions.MapJsonKey(ClaimTypes.Webpage, "html_url", ClaimValueTypes.String);
            this.ClaimActions.MapJsonKey("picture", "avatar_url", ClaimValueTypes.String);
            this.ClaimActions.MapJsonKey("public_repos", "public_repos", ClaimValueTypes.Integer);
            this.ClaimActions.MapJsonKey("followers", "followers", ClaimValueTypes.Integer);
            this.ClaimActions.MapJsonKey("created", "created_at", ClaimValueTypes.DateTime);
            this.ClaimActions.MapJsonKey("bio", "bio", ClaimValueTypes.String);
            this.ClaimActions.MapJsonKey("blog", "blog", ClaimValueTypes.String);
            this.ClaimActions.MapJsonKey("location", "location", ClaimValueTypes.String);
        }
    }
}
