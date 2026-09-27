namespace MintPlayer.Web.Server.ViewModels.Account
{
    public class TwoFactorBypassVM
    {
        public string SetupCode { get; set; }
        public bool Bypass2faForExternalLogins { get; set; }
    }
}
