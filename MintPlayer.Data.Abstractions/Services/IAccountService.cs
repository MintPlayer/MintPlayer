using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using MintPlayer.Dtos.Dtos;

namespace MintPlayer.Data.Abstractions.Services;

public interface IAccountService
{
	Task Register(User user, string password);
	Task SendConfirmationEmail(string email);
	Task VerifyEmailConfirmationToken(string email, string token);
	Task<string> GeneratePasswordResetToken(string email);
	Task SendPasswordResetEmail(string email, string resetUrl);
	Task ResetPassword(string email, string token, string newPassword);
	Task<LocalLoginResult> LocalLogin(string email, string password, bool createCookie);
	Task<IEnumerable<string>> GetProviders();
	Task<AuthenticationProperties> ConfigureExternalAuthenticationProperties(string provider, string redirectUrl);
	Task<ExternalLoginResult> PerfromExternalLogin();
	Task<IEnumerable<UserLoginInfo>> GetExternalLogins(ClaimsPrincipal userProperty);
	Task AddExternalLogin(ClaimsPrincipal userProperty);
	Task RemoveExternalLogin(ClaimsPrincipal userProperty, string provider);
	Task<User> GetCurrentUser(ClaimsPrincipal userProperty);
	Task<bool> GetHasPassword(ClaimsPrincipal userProperty);
	Task UpdatePassword(ClaimsPrincipal userProperty, string currentPassword, string newPassword, string confirmation);
	Task<IEnumerable<string>> GetCurrentRoles(ClaimsPrincipal userProperty);
	Task Logout();

	Task<string> GenerateTwoFactorRegistrationCode(ClaimsPrincipal userProperty);
	Task<IEnumerable<string>> GenerateTwoFactorBackupCodes(ClaimsPrincipal userProperty);
	Task SetTwoFactorEnabled(ClaimsPrincipal userProperty, string code, bool enable);
	Task SetTwoFactorBypass(ClaimsPrincipal userProperty, bool bypass, string code);
	Task<User> TwoFactorLogin(string authenticatorCode, bool remember);
	Task TwoFactorRecovery(string backupCode);
	Task<int> GetRemainingNumberOfRecoveryCodes(ClaimsPrincipal userProperty);
}
