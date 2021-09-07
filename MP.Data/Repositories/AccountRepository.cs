using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Exceptions.Account;
using MintPlayer.Data.Exceptions.Account.ExternalLogin;
using MintPlayer.Data.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MintPlayer.Dtos.Enums;
using MintPlayer.Data.Exceptions.Account.TwoFactor;
using MintPlayer.Data.Mappers;

namespace MintPlayer.Data.Repositories
{
    internal interface IAccountRepository
    {
        Task<User> Register(User user, string password);
        Task<string> GenerateEmailConfirmationToken(string email);
        Task VerifyEmailConfirmationToken(string email, string token);
        Task<string> GeneratePasswordResetToken(string email);
        Task ResetPassword(string email, string token, string newPassword);
        Task<LocalLoginResult> LocalLogin(string email, string password, bool createCookie);
        Task<IEnumerable<AuthenticationScheme>> GetExternalLoginProviders();
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

        Task<User> GetTwoFactorUser();
        Task<string> GenerateTwoFactorRegistrationCode(ClaimsPrincipal userProperty);
        Task<IEnumerable<string>> GenerateTwoFactorBackupCodes(ClaimsPrincipal userProperty);
        Task SetTwoFactorEnabled(ClaimsPrincipal userProperty, string code, bool enable);
        Task SetTwoFactorBypass(ClaimsPrincipal userProperty, bool bypass, string code);
        Task<User> TwoFactorLogin(string authenticatorCode, bool remember);
		Task TwoFactorRecovery(string backupCode);
		Task<int> GetRemainingNumberOfRecoveryCodes(ClaimsPrincipal userProperty);
	}
	internal class AccountRepository : IAccountRepository
    {
        private MintPlayerContext mintplayer_context;
        private readonly IUserMapper userMapper;
        private UserManager<Entities.User> user_manager;
        private SignInManager<Entities.User> signin_manager;
        private JwtIssuerOptions jwtIssuerOptions;
        public AccountRepository(
            UserManager<Entities.User> user_manager,
            SignInManager<Entities.User> signin_manager,
            MintPlayerContext mintplayer_context,
            IOptions<JwtIssuerOptions> jwtIssuerOptions,
            IUserMapper userMapper)
        {
            this.user_manager = user_manager;
            this.signin_manager = signin_manager;
            this.mintplayer_context = mintplayer_context;
            this.userMapper = userMapper;
            this.jwtIssuerOptions = jwtIssuerOptions.Value;
        }

        public async Task<User> Register(User user, string password)
        {
            try
            {
                var entity_user = userMapper.Dto2Entity(user);
                var identity_result = await user_manager.CreateAsync(entity_user, password);
                if (identity_result.Succeeded)
                {
                    var dto_user = userMapper.Entity2Dto(entity_user, true);
                    return dto_user;
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

        public async Task<string> GenerateEmailConfirmationToken(string email)
        {
            var user = await user_manager.FindByEmailAsync(email);
            var token = await user_manager.GenerateEmailConfirmationTokenAsync(user);
            return token;
        }

        public async Task VerifyEmailConfirmationToken(string email, string token)
        {
            var user = await user_manager.FindByEmailAsync(email);
            var result = await user_manager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                throw new VerifyEmailException(
                    new Exception(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)))
                );
            }
        }

        public async Task<string> GeneratePasswordResetToken(string email)
        {
            var user = await user_manager.FindByEmailAsync(email);
            var token = await user_manager.GeneratePasswordResetTokenAsync(user);
            return token;
        }

        public async Task ResetPassword(string email, string token, string newPassword)
        {
            var user = await user_manager.FindByEmailAsync(email);
            var result = await user_manager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                throw new ResetPasswordException(
                    new Exception(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)))
                );
            }
        }

        public async Task<LocalLoginResult> LocalLogin(string email, string password, bool createCookie)
        {
            var user = await user_manager.FindByEmailAsync(email);
            if (user == null)
                throw new LoginException();

            var checkPasswordResult = await signin_manager.CheckPasswordSignInAsync(user, password, true);
            if (!checkPasswordResult.Succeeded)
            {
                throw new LoginException();
            }

            var isEmailConfirmed = await user_manager.IsEmailConfirmedAsync(user);
            if (!isEmailConfirmed)
            {
                throw new EmailNotConfirmedException();
            }

            if (createCookie)
            {
                var signinResult = await signin_manager.PasswordSignInAsync(user, password, true, true);
                if (signinResult.Succeeded)
                {
                    return new LocalLoginResult
                    {
                        Status = LoginStatus.Success,
                        User = userMapper.Entity2Dto(user, true)
                    };
                }

                if (signinResult.RequiresTwoFactor)
                {
                    return new LocalLoginResult
                    {
                        Status = LoginStatus.RequiresTwoFactor,
                        User = userMapper.Entity2Dto(user, true)
                    };
                }

                // Login failed, but not because it required TwoFactor authentication
                throw new LoginException();
            }
            else
            {
                var checkpassword = await user_manager.CheckPasswordAsync(user, password);
                if (!checkpassword)
                {
                    throw new LoginException();
                }

                if (user.TwoFactorEnabled)
                {
                    return new LocalLoginResult
                    {
                        Status = LoginStatus.RequiresTwoFactor,
                        User = userMapper.Entity2Dto(user, true)
                    };
                }

                return new LocalLoginResult
                {
                    Status = LoginStatus.Success,
                    User = userMapper.Entity2Dto(user, true),
                    Token = CreateToken(user)
                };
            }
        }

        public async Task<IEnumerable<AuthenticationScheme>> GetExternalLoginProviders()
        {
			// https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade
			// https://console.developers.google.com
			// https://developers.facebook.com/apps
			// http://developer.twitter.com/en/apps
			// https://www.linkedin.com/developers/apps

			var providers = await signin_manager.GetExternalAuthenticationSchemesAsync();
            return providers.ToList();
        }

        public Task<AuthenticationProperties> ConfigureExternalAuthenticationProperties(string provider, string redirectUrl)
        {
            var properties = signin_manager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Task.FromResult(properties);
        }

        public async Task<ExternalLoginResult> PerfromExternalLogin()
        {
            var info = await signin_manager.GetExternalLoginInfoAsync();
            if (info == null)
                throw new UnauthorizedAccessException();

            var user = await user_manager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user == null)
            {
                string username = info.Principal.FindFirstValue(ClaimTypes.Name);
                string email = info.Principal.FindFirstValue(ClaimTypes.Email);

                var new_user = new Entities.User
                {
                    UserName = username,
                    Email = email,
                    PictureUrl = null,
                    EmailConfirmed = true,
                };
                var id_result = await user_manager.CreateAsync(new_user);
                if (id_result.Succeeded)
                {
                    user = new_user;
                }
                else
                {
                    // User creation failed, probably because the email address is already present in the database
                    if (id_result.Errors.Any(e => e.Code == "DuplicateEmail"))
                    {
                        var existing = await user_manager.FindByEmailAsync(email);
                        var existing_logins = await user_manager.GetLoginsAsync(existing);

                        if (existing_logins.Any())
                        {
                            throw new OtherAccountException(existing_logins);
                        }
                        else
                        {
                            throw new Exception("Could not create account from social profile");
                        }
                    }
                    else
                    {
                        throw new Exception("Could not create account from social profile");
                    }
                }

                await user_manager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
            }

            var signinResult = await signin_manager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true, user.Bypass2faForExternalLogin);

            if (signinResult.Succeeded)
            {
                return new ExternalLoginResult
                {
                    Status = LoginStatus.Success,
                    Platform = info.LoginProvider,
                    User = userMapper.Entity2Dto(user, true),
                };
            }
            else if (signinResult.RequiresTwoFactor)
            {
                return new ExternalLoginResult
                {
                    Status = LoginStatus.RequiresTwoFactor,
                    Platform = info.LoginProvider,
                    User = userMapper.Entity2Dto(user, true),
                };
            }
            else
            {
                return new ExternalLoginResult
                {
                    Status = LoginStatus.Failed,
                };
            }
        }

        public async Task<IEnumerable<UserLoginInfo>> GetExternalLogins(ClaimsPrincipal userProperty)
        {
            // Get current user
            var user = await user_manager.GetUserAsync(userProperty);
            if (user == null) throw new UnauthorizedAccessException();

            var user_logins = await user_manager.GetLoginsAsync(user);
            return user_logins;
        }

        public async Task AddExternalLogin(ClaimsPrincipal userProperty)
        {
            // Get current user
            var user = await user_manager.GetUserAsync(userProperty);
            if (user == null) throw new UnauthorizedAccessException();

            // Get login info
            var info = await signin_manager.GetExternalLoginInfoAsync();
            if (info == null) throw new UnauthorizedAccessException();

            var result = await user_manager.AddLoginAsync(user, info);
            if (!result.Succeeded) throw new Exception(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
        }

        public async Task RemoveExternalLogin(ClaimsPrincipal userProperty, string provider)
        {
            // Get current user
            var user = await user_manager.GetUserAsync(userProperty);
            if (user == null) throw new UnauthorizedAccessException();

            var user_logins = await user_manager.GetLoginsAsync(user);
            var login = user_logins.FirstOrDefault(l => l.LoginProvider == provider);

            if (login == null) throw new InvalidOperationException($"Could not remove {provider} login");

            var result = await user_manager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
            if (!result.Succeeded) throw new Exception($"Could not remove {provider} login");
        }

        public async Task<User> GetCurrentUser(ClaimsPrincipal userProperty)
        {
            var user = await user_manager.GetUserAsync(userProperty);
            return userMapper.Entity2Dto(user, true);
        }

        public async Task<bool> GetHasPassword(ClaimsPrincipal userProperty)
        {
            var user = await user_manager.GetUserAsync(userProperty);
            var hasPassword = await user_manager.HasPasswordAsync(user);
            return hasPassword;
        }

        public async Task UpdatePassword(ClaimsPrincipal userProperty, string currentPassword, string newPassword, string confirmation)
        {
            if (newPassword != confirmation)
            {
                throw new ChangePasswordException();
            }

            var user = await user_manager.GetUserAsync(userProperty);

            IdentityResult result;
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                result = await user_manager.AddPasswordAsync(user, newPassword);
            }
            else
            {
                result = await user_manager.ChangePasswordAsync(user, currentPassword ?? string.Empty, newPassword);
            }

            if (!result.Succeeded)
            {
                throw new ChangePasswordException(
                    new Exception(
                        string.Join(
                            Environment.NewLine,
                            result.Errors.Select(e => e.Description)
                        )
                    )
                );
            }
        }

        public async Task<IEnumerable<string>> GetCurrentRoles(ClaimsPrincipal userProperty)
        {
            var user = await user_manager.GetUserAsync(userProperty);
            var roles = await user_manager.GetRolesAsync(user);
            return roles;
        }

        public async Task Logout()
        {
            await signin_manager.SignOutAsync();
        }




        public async Task<User> GetTwoFactorUser()
        {
            var user = await signin_manager.GetTwoFactorAuthenticationUserAsync();
            return userMapper.Entity2Dto(user, true);
        }

        public async Task<string> GenerateTwoFactorRegistrationCode(ClaimsPrincipal userProperty)
        {
            //var user = await signin_manager.GetTwoFactorAuthenticationUserAsync();
            var user = await user_manager.GetUserAsync(userProperty);

            var code = await user_manager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(code))
            {
                await user_manager.ResetAuthenticatorKeyAsync(user);
                code = await user_manager.GetAuthenticatorKeyAsync(user);
            }

            return code;
        }

        public async Task<IEnumerable<string>> GenerateTwoFactorBackupCodes(ClaimsPrincipal userProperty)
        {
            var user = await user_manager.GetUserAsync(userProperty);
            var codes = await user_manager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            return codes;
        }

        public async Task SetTwoFactorEnabled(ClaimsPrincipal userProperty, string code, bool enable)
        {
            var user = await user_manager.GetUserAsync(userProperty);
            var is2faTokenValid = await user_manager.VerifyTwoFactorTokenAsync(user, user_manager.Options.Tokens.AuthenticatorTokenProvider, code);

            if (!is2faTokenValid)
            {
                throw new InvalidTwoFactorCodeException();
            }

            var result = await user_manager.SetTwoFactorEnabledAsync(user, enable);
            if (!result.Succeeded)
            {
                throw new TwoFactorSetupException();
            }
        }

        public async Task SetTwoFactorBypass(ClaimsPrincipal userProperty, bool bypass, string code)
        {
            var user = await user_manager.GetUserAsync(userProperty);
            var is2faTokenValid = await user_manager.VerifyTwoFactorTokenAsync(user, user_manager.Options.Tokens.AuthenticatorTokenProvider, code);

            if (!is2faTokenValid)
            {
                throw new InvalidTwoFactorCodeException();
            }

            user.Bypass2faForExternalLogin = bypass;
            await mintplayer_context.SaveChangesAsync();
        }

        public async Task<User> TwoFactorLogin(string authenticatorCode, bool remember)
        {
            var user = await signin_manager.GetTwoFactorAuthenticationUserAsync();
            var result = await signin_manager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, true, remember);
            if (result.Succeeded)
            {
                return userMapper.Entity2Dto(user, true);
            }
            else
            {
                throw new LoginException();
            }
        }

		public async Task TwoFactorRecovery(string backupCode)
		{
			var result = await signin_manager.TwoFactorRecoveryCodeSignInAsync(backupCode);
			if (!result.Succeeded)
			{
				throw new TwoFactorRecoveryException();
			}
		}

		public async Task<int> GetRemainingNumberOfRecoveryCodes(ClaimsPrincipal userProperty)
		{
			var user = await user_manager.GetUserAsync(userProperty);
			var result = await user_manager.CountRecoveryCodesAsync(user);
			return result;
		}

		#region Helper methods
		private string CreateToken(Entities.User user)
        {
            var token_descriptor = new SecurityTokenDescriptor
            {
                Issuer = jwtIssuerOptions.Issuer,
                IssuedAt = DateTime.UtcNow,
                Audience = jwtIssuerOptions.Audience,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.Add(jwtIssuerOptions.ValidFor),
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                SigningCredentials = new Func<SigningCredentials>(() =>
                {
                    var bytes = Encoding.UTF8.GetBytes(jwtIssuerOptions.Key);
                    var signing_key = new SymmetricSecurityKey(bytes);
                    var signing_credentials = new SigningCredentials(signing_key, SecurityAlgorithms.HmacSha256Signature);
                    return signing_credentials;
                }).Invoke()
            };

            var token_handler = new JwtSecurityTokenHandler();
            var token = token_handler.CreateToken(token_descriptor);
            var str_token = token_handler.WriteToken(token);

            return str_token;
        }
        #endregion
    }
}
