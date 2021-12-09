using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MintPlayer.Data.Exceptions.Account
{
	public class RegistrationException : Exception
	{
		private RegistrationException() : base("Could not create user account")
		{
		}
		public RegistrationException(Exception inner) : base("Could not create user account", inner)
		{
		}
		public RegistrationException(IEnumerable<IdentityError> errors) : this()
		{
			Errors = errors;
		}
		public RegistrationException(IEnumerable<IdentityError> errors, Exception inner) : this(inner)
		{
			Errors = errors;
		}

		public IEnumerable<IdentityError> Errors { get; private set; }
	}
}
