namespace MintPlayer.Web.Server.ViewModels.Account;

public class ResetPasswordVM
{
	public string Email { get; set; }
	public string Token { get; set; }
	public string NewPassword { get; set; }
	public string NewPasswordConfirmation { get; set; }
}
