using Identity.ExternalProviders.GitHub;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;

namespace MintPlayer.Data.Options
{
    public class MintPlayerOptions
    {
        public string ConnectionString { get; set; }
        public JwtIssuerOptions JwtIssuerOptions { get; set; }
        public FacebookOptions FacebookOptions { get; set; }
        public MicrosoftAccountOptions MicrosoftOptions { get; set; }
        public GoogleOptions GoogleOptions { get; set; }
        public TwitterOptions TwitterOptions { get; set; }
        public GitHubOptions GitHubOptions { get; set; }
    }
}
