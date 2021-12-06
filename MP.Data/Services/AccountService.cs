using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using System.Net.Mail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MintPlayer.Data.Abstractions.Services;

namespace MintPlayer.Data.Services
{
	internal class AccountService : IAccountService
    {
        private readonly IAccountRepository accountRepository;
        private readonly IMailService mailService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly LinkGenerator linkGenerator;
        public AccountService(IAccountRepository accountRepository, IMailService mailService, IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        {
            this.accountRepository = accountRepository;
            this.mailService = mailService;
            this.httpContextAccessor = httpContextAccessor;
            this.linkGenerator = linkGenerator;
        }

        public async Task Register(User user, string password)
        {
            var newUser = await accountRepository.Register(user, password);
            await SendConfirmationEmail(user.Email);
        }

        public async Task SendConfirmationEmail(string email)
        {
            await Task.Run(async () =>
            {
                using (var client = await mailService.CreateSmtpClient())
                {
                    var code = await accountRepository.GenerateEmailConfirmationToken(email);
                    var url = linkGenerator.GetUriByName("web-v3-account-verify", new { email, code = Base64UrlTextEncoder.Encode(System.Text.Encoding.UTF8.GetBytes(code)) }, httpContextAccessor.HttpContext.Request.Scheme, httpContextAccessor.HttpContext.Request.Host);

                    var html = $@"Please confirm your account by clicking <a href=""{url}"">here</a>.";
                    using (var message = new MailMessage("no-reply@mintplayer.com", email, "Confirm email address", html))
                    {
                        message.IsBodyHtml = true;
                        client.Send(message);
                    }
                }
            });
        }

        public async Task VerifyEmailConfirmationToken(string email, string token)
        {
            await accountRepository.VerifyEmailConfirmationToken(email, token);
        }

        public async Task<string> GeneratePasswordResetToken(string email)
        {
            var token = await accountRepository.GeneratePasswordResetToken(email);
            return token;
        }

        public async Task SendPasswordResetEmail(string email, string resetUrl)
        {
            await Task.Run(async () =>
            {
                using (var client = await mailService.CreateSmtpClient())
                {
                    var html = $@"You can reset your password through the <a href=""{resetUrl}"">following link</a>";
                    using (var message = new MailMessage("no-reply@mintplayer.com", email, "Reset password", html))
                    {
                        message.IsBodyHtml = true;
                        client.Send(message);
                    }
                }
            });
        }

        public async Task ResetPassword(string email, string token, string newPassword)
        {
            await accountRepository.ResetPassword(email, token, newPassword);
        }

        public async Task<LocalLoginResult> LocalLogin(string email, string password, bool createCookie)
        {
            var login_result = await accountRepository.LocalLogin(email, password, createCookie);
            return login_result;
        }

        public async Task<IEnumerable<string>> GetProviders()
        {
            var result = await accountRepository.GetExternalLoginProviders();
            return result.Select(s => s.DisplayName);
        }

        public async Task<AuthenticationProperties> ConfigureExternalAuthenticationProperties(string provider, string redirectUrl)
        {
            var properties = await accountRepository.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return properties;
        }

        public async Task<ExternalLoginResult> PerfromExternalLogin()
        {
            var loginResult = await accountRepository.PerfromExternalLogin();
            return loginResult;
        }

        public async Task<IEnumerable<UserLoginInfo>> GetExternalLogins(ClaimsPrincipal userProperty)
        {
            var userLogins = await accountRepository.GetExternalLogins(userProperty);
            return userLogins;
        }

        public async Task AddExternalLogin(ClaimsPrincipal userProperty)
        {
            await accountRepository.AddExternalLogin(userProperty);
        }

        public async Task RemoveExternalLogin(ClaimsPrincipal userProperty, string provider)
        {
            await accountRepository.RemoveExternalLogin(userProperty, provider);
        }

        public async Task<bool> GetHasPassword(ClaimsPrincipal userProperty)
        {
            var hasPassword = await accountRepository.GetHasPassword(userProperty);
            return hasPassword;
        }

        public async Task UpdatePassword(ClaimsPrincipal userProperty, string currentPassword, string newPassword, string confirmation)
        {
            await accountRepository.UpdatePassword(userProperty, currentPassword, newPassword, confirmation);
        }

        public async Task<User> GetCurrentUser(ClaimsPrincipal userProperty)
        {
            var user = await accountRepository.GetCurrentUser(userProperty);
            return user;
        }

        public Task<IEnumerable<string>> GetCurrentRoles(ClaimsPrincipal userProperty)
        {
            return accountRepository.GetCurrentRoles(userProperty);
        }

        public async Task Logout()
        {
            await accountRepository.Logout();
        }

        public async Task<string> GenerateTwoFactorRegistrationCode(ClaimsPrincipal userProperty)
        {
            var registrationCode = await accountRepository.GenerateTwoFactorRegistrationCode(userProperty);
            return registrationCode;
        }

        public async Task<IEnumerable<string>> GenerateTwoFactorBackupCodes(ClaimsPrincipal userProperty)
        {
            var codes = await accountRepository.GenerateTwoFactorBackupCodes(userProperty);
            return codes;
        }

        public async Task SetTwoFactorEnabled(ClaimsPrincipal userProperty, string code, bool enable)
        {
            await accountRepository.SetTwoFactorEnabled(userProperty, code, enable);
        }

        public async Task SetTwoFactorBypass(ClaimsPrincipal userProperty, bool bypass, string code)
        {
            await accountRepository.SetTwoFactorBypass(userProperty, bypass, code);
        }

        public async Task<User> TwoFactorLogin(string authenticatorCode, bool remember)
        {
            var result = await accountRepository.TwoFactorLogin(authenticatorCode, remember);
            return result;
        }

		public async Task TwoFactorRecovery(string backupCode)
		{
			await accountRepository.TwoFactorRecovery(backupCode);
		}

		public async Task<int> GetRemainingNumberOfRecoveryCodes(ClaimsPrincipal userProperty)
		{
			var result = await accountRepository.GetRemainingNumberOfRecoveryCodes(userProperty);
			return result;
		}
	}
}
