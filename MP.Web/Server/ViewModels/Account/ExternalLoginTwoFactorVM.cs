namespace MintPlayer.Web.Server.ViewModels.Account
{
    public class ExternalLoginTwoFactorVM
    {
        public string SubmitUrl { get; set; }
        public string Code { get; set; }
        public bool Remember { get; set; }
    }
}
