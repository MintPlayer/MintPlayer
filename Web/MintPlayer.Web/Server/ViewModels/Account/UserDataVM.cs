using MintPlayer.Data.Dtos;

namespace MintPlayer.Web.Server.ViewModels.Account
{
    public class UserDataVM
    {
        public User User { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
    }
}
