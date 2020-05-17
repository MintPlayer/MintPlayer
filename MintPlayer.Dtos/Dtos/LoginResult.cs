namespace MintPlayer.Dtos.Dtos
{
	public class LoginResult
	{
		public bool Status { get; set; }
		public string Platform { get; set; }

		public User User { get; set; }
		public string Token { get; set; }

		public string Error { get; set; }
		public string ErrorDescription { get; set; }
	}
}
