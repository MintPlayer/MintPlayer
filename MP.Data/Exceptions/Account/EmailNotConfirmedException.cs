using System;

namespace MintPlayer.Data.Exceptions.Account
{
	public class EmailNotConfirmedException : Exception
	{
		public EmailNotConfirmedException() : base("Your email address is not confirmed")
		{
		}
		public EmailNotConfirmedException(Exception inner) : base("Your email address is not confirmed", inner)
		{
		}
	}
}
