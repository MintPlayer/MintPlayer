namespace MintPlayer.Data.Exceptions.Paging;

public class InvalidSortPropertyException : Exception
{
	public InvalidSortPropertyException() : base("The specified sorting property does not exist.")
	{
	}
}
