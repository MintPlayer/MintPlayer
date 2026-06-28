using System;

namespace MintPlayer.Data.Exceptions.Account.TwoFactor
{
    public class TwoFactorSetupException : Exception
    {
        public TwoFactorSetupException() : base("Failed to enable/disable two-factor authentication")
        {
        }
    }
}
