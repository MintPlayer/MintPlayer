namespace MintPlayer.Data.Exceptions.Account;

public class ChangePasswordException : Exception
{
	public ChangePasswordException() : base("Could not update the password")
	{
	}
	public ChangePasswordException(Exception inner) : base("Could not update the password", inner)
	{
	}
}
