using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;

namespace MintPlayer.Data.Options
{
    public class MintPlayerOptions
    {
        public string ConnectionString { get; set; }
        public JwtIssuerOptions JwtIssuerOptions { get; set; }
        public FacebookOptions FacebookOptions { get; set; }
        public MicrosoftAccountOptions MicrosoftOptions { get; set; }
    }
}
