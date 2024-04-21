namespace MintPlayer.Data.Exceptions;

public class ConcurrencyException<TDto> : Exception
{
	internal ConcurrencyException() : base("The entity was updated during the edit operation") { }

	public TDto DatabaseValue { get; init; }
}

public static class ConcurrencyException
{
	public static ConcurrencyException<T> Create<T>(T dto)
	{
		return new ConcurrencyException<T> { DatabaseValue = dto };
	}
}
