namespace MintPlayer.Data.Exceptions.Account;

public class VerifyEmailException : Exception
{
	public VerifyEmailException() : base("Email verification failed")
	{
	}
	public VerifyEmailException(Exception inner) : base("Email verification failed", inner)
	{
	}
}
