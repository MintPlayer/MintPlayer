using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using MintPlayer.Data.Dtos;
using Microsoft.AspNetCore.Identity;

namespace MintPlayer.Data.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<Tuple<User, string>> Register(User user, string password);
        Task<LoginResult> LocalLogin(string email, string password, bool remember);
        Task<IEnumerable<AuthenticationScheme>> GetExternalLoginProviders();
        AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl);
        Task<LoginResult> PerfromExternalLogin();
        Task<IEnumerable<UserLoginInfo>> GetExternalLogins(ClaimsPrincipal userProperty);
        Task AddExternalLogin(ClaimsPrincipal userProperty);
        Task RemoveExternalLogin(ClaimsPrincipal userProperty, string provider);
        Task<User> GetCurrentUser(ClaimsPrincipal userProperty);
        Task Logout();
    }
}
