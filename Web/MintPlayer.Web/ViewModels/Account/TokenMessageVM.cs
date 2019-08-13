namespace MintPlayer.Web.ViewModels.Account
{
    public class TokenMessageVM
    {
        public string AccessToken { get; set; }
        public string Platform { get; set; }

        public string Error { get; set; }
        public string ErrorDescription { get; set; }
    }
}
