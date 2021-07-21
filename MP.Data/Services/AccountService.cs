using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using MintPlayer.Data.Options;
using MintPlayer.Data.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MintPlayer.Data.Services
{
    public interface IAccountService
    {
        Task Register(User user, string password);
        Task SendConfirmationEmail(string email);
        Task VerifyEmailConfirmationToken(string email, string token);
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
        Task FinishTwoFactorSetup(ClaimsPrincipal userProperty, string code);
        Task TwoFactorDisable(ClaimsPrincipal userProperty, string code);
        Task SetTwoFactorBypass(ClaimsPrincipal userProperty, bool bypass, string code);
        Task<User> TwoFactorLogin(string authenticatorCode, bool remember);
    }

    internal class AccountService : IAccountService
    {
        private readonly IAccountRepository accountRepository;
        private readonly IOptions<SmtpOptions> smtpOptions;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly LinkGenerator linkGenerator;
        public AccountService(IAccountRepository accountRepository, IOptions<Options.SmtpOptions> smtpOptions, IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        {
            this.accountRepository = accountRepository;
            this.smtpOptions = smtpOptions;
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
                using (var client = new SmtpClient())
                {
                    client.Connect(smtpOptions.Value.Host, smtpOptions.Value.Port, smtpOptions.Value.UseTLS);
                    client.Credentials = new System.Net.NetworkCredential(smtpOptions.Value.User, smtpOptions.Value.Password);

                    var code = await accountRepository.GenerateEmailConfirmationToken(email);
                    var url = this.linkGenerator.GetUriByName("web-v3-account-verify", new { email, code = Base64UrlTextEncoder.Encode(System.Text.Encoding.UTF8.GetBytes(code)) }, httpContextAccessor.HttpContext.Request.Scheme, httpContextAccessor.HttpContext.Request.Host);

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

        public async Task FinishTwoFactorSetup(ClaimsPrincipal userProperty, string code)
        {
            await accountRepository.FinishTwoFactorSetup(userProperty, code);
        }

        public async Task TwoFactorDisable(ClaimsPrincipal userProperty, string code)
        {
            await accountRepository.TwoFactorDisable(userProperty, code);
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
    }
}