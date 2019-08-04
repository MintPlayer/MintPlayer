namespace MintPlayer.Data.Dtos
{
    public class LoginResult
    {
        public bool Status { get; set; }
        public string Platform { get; set; }

        public Dtos.User User { get; set; }
        public string Token { get; set; }

        public string Error { get; set; }
        public string ErrorDescription { get; set; }
    }
}
