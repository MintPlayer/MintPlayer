namespace MintPlayer.Web.Server.ViewModels.Account
{
    public class WebAuthnRegisterResponseVM
    {
        public string Id { get; set; }
        public string RawId { get; set; }
        public string AttestationObject { get; set; }
        public string ClientDataJSON { get; set; }
        public string[] Transports { get; set; }
        public string DisplayName { get; set; }
    }
}
