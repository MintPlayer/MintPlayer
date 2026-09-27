namespace MintPlayer.Web.Server.ViewModels.Account
{
    public class WebAuthnLoginOptionsVM
    {
        /// <summary>
        /// Optional email for non-discoverable credentials flow.
        /// If empty, discoverable credentials (resident keys) will be used.
        /// </summary>
        public string Email { get; set; }
    }
}
