using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MintPlayer.Data.Exceptions.Account;
using MintPlayer.Data.Repositories.Interfaces;

namespace MintPlayer.Data.Repositories
{
    internal class AccountRepository : IAccountRepository
    {
        private MintPlayerContext mintplayer_context;
        private UserManager<Entities.User> user_manager;
        public AccountRepository(UserManager<Entities.User> user_manager, MintPlayerContext mintplayer_context)
        {
            this.user_manager = user_manager;
            this.mintplayer_context = mintplayer_context;
        }

        public async Task<Tuple<Dtos.User, string>> Register(Dtos.User user, string password)
        {
            try
            {
                var entity_user = ToEntity(user);
                var identity_result = await user_manager.CreateAsync(entity_user, password);
                if (identity_result.Succeeded)
                {
                    var dto_user = ToDto(entity_user);
                    var confirmation_token = await user_manager.GenerateEmailConfirmationTokenAsync(entity_user);
                    return new Tuple<Dtos.User, string>(dto_user, confirmation_token);
                }
                else
                {
                    throw new RegistrationException(identity_result.Errors);
                }
            }
            catch (RegistrationException RegistrationEx)
            {
                throw RegistrationEx;
            }
            catch (Exception ex)
            {
                throw new RegistrationException(ex);
            }
        }

        #region Conversion methods
        internal static Entities.User ToEntity(Dtos.User user)
        {
            if (user == null) return null;
            return new Entities.User
            {
                Id = user.Id ?? 0,
                Email = user.Email,
                UserName = user.UserName,
                PictureUrl = user.PictureUrl
            };
        }
        internal static Dtos.User ToDto(Entities.User user)
        {
            if (user == null) return null;
            return new Dtos.User
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                PictureUrl = user.PictureUrl
            };
        }
        #endregion
    }
}
