using System;

namespace MintPlayer.Data.Exceptions.Account
{
    public class ResetPasswordException : Exception
    {
        public ResetPasswordException() : base("Reset password failed")
        {
        }

        public ResetPasswordException(Exception inner) : base ("Reset password failed", inner)
        {
        }
    }
}
