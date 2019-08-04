using System;
using System.Threading.Tasks;

namespace MintPlayer.Data.Repositories.Interfaces
{

    public interface IAccountRepository
    {
        Task<Tuple<Dtos.User, string>> Register(Dtos.User user, string password);
    }
}
