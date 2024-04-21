namespace MintPlayer.Data.Exceptions.Account.TwoFactor;

public class TwoFactorRecoveryException : Exception
{
	public TwoFactorRecoveryException() : base("There was an error recovering your account with the code specified")
	{
	}
}
