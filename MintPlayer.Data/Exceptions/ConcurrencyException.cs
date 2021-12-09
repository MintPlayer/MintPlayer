using System;

namespace MintPlayer.Data.Exceptions
{
    public class ConcurrencyException<TDto> : Exception
    {
        internal ConcurrencyException(TDto databaseValue) : base("The entity was updated during the edit operation")
        {
            DatabaseValue = databaseValue;
        }

        public TDto DatabaseValue { get; }
    }

    public static class ConcurrencyException
    {
        public static ConcurrencyException<T> Create<T>(T dto)
        {
            return new ConcurrencyException<T>(dto);
        }
    }
}
