namespace MintPlayer.Data.Exceptions.Account.TwoFactor;

public class InvalidTwoFactorCodeException : Exception
{
	public InvalidTwoFactorCodeException() : base("The specified two-factor code is invalid")
	{
	}
}
