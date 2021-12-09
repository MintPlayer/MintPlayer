using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MintPlayer.Data.Exceptions.Account.ExternalLogin
{
	public class OtherAccountException : Exception
	{
		public OtherAccountException(IList<UserLoginInfo> existing_logins)
			: base($"Please login with your {existing_logins[0].ProviderDisplayName} account instead")
		{
		}
	}
}
