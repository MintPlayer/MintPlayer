namespace MintPlayer.Data.Exceptions;

public class NotFoundException : Exception
{
	public NotFoundException() : base("The item was not found")
	{
	}
}
