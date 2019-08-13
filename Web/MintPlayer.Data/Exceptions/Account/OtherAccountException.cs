using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace MintPlayer.Data.Exceptions.Account
{
    public class OtherAccountException : Exception
    {
        public OtherAccountException(IList<UserLoginInfo> existing_logins)
            : base($"Please login with your {existing_logins[0].ProviderDisplayName} account instead")
        {
        }
    }
}
