using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Repositories;

namespace MintPlayer.Data.Services
{
    public interface IAccountService
    {
        Task Register(User user, string password);
        Task<LoginResult> LocalLogin(string email, string password, bool createCookie);
        Task<IEnumerable<string>> GetProviders();
        Task<AuthenticationProperties> ConfigureExternalAuthenticationProperties(string provider, string redirectUrl);
        Task<LoginResult> PerfromExternalLogin();
        Task<IEnumerable<UserLoginInfo>> GetExternalLogins(ClaimsPrincipal userProperty);
        Task AddExternalLogin(ClaimsPrincipal userProperty);
        Task RemoveExternalLogin(ClaimsPrincipal userProperty, string provider);
        Task<User> GetCurrentUser(ClaimsPrincipal userProperty);
        Task<bool> GetHasPassword(ClaimsPrincipal userProperty);
        Task UpdatePassword(ClaimsPrincipal userProperty, string currentPassword, string newPassword, string confirmation);
        Task<IEnumerable<string>> GetCurrentRoles(ClaimsPrincipal userProperty);
        Task Logout();
    }

    internal class AccountService : IAccountService
    {
        private IAccountRepository accountRepository;
        public AccountService(IAccountRepository accountRepository)
        {
            this.accountRepository = accountRepository;
        }

        public async Task Register(User user, string password)
        {
            var registrationData = await accountRepository.Register(user, password);
            //var newUser = registrationData.Item1;
            //var confirmationToken = registrationData.Item2;
        }

        public async Task<LoginResult> LocalLogin(string email, string password, bool createCookie)
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

        public async Task<LoginResult> PerfromExternalLogin()
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
    }
}