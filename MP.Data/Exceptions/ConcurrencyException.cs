using System;

namespace MintPlayer.Data.Exceptions
{
    public class ConcurrencyException : Exception
    {
        internal ConcurrencyException() : base("The entity was updated during the edit operation")
        {
        }
    }
}
