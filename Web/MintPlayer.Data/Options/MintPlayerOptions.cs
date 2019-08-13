using Microsoft.AspNetCore.Authentication.Facebook;

namespace MintPlayer.Data.Options
{
    public class MintPlayerOptions
    {
        public string ConnectionString { get; set; }
        public JwtIssuerOptions JwtIssuerOptions { get; set; }
        public FacebookOptions FacebookOptions { get; set; }
    }
}
