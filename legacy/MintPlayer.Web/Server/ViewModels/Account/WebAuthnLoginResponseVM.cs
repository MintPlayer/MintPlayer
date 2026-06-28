namespace MintPlayer.Web.Server.ViewModels.Account
{
    public class WebAuthnLoginResponseVM
    {
        public string Id { get; set; }
        public string RawId { get; set; }
        public string ClientDataJSON { get; set; }
        public string AuthenticatorData { get; set; }
        public string Signature { get; set; }
        public string UserHandle { get; set; }
    }
}
