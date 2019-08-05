using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MintPlayer.Data.Repositories.Interfaces
{

    public interface IAccountRepository
    {
        Task<Tuple<Dtos.User, string>> Register(Dtos.User user, string password);
        Task<Dtos.LoginResult> LocalLogin(string email, string password, bool remember);
        Task<Dtos.User> GetCurrentUser(ClaimsPrincipal userProperty);
        Task Logout();
    }
}
