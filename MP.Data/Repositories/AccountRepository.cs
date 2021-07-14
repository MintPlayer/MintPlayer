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
using Microsoft.AspNetCore.Http;

namespace MintPlayer.Data.Repositories
{
    internal interface IAccountRepository
    {
        Task<Tuple<User, string>> Register(User user, string password);
		Task<LoginResult> LocalLogin(string email, string password, bool createCookie);
		Task<IEnumerable<AuthenticationScheme>> GetExternalLoginProviders();
		Task<AuthenticationProperties> ConfigureExternalAuthenticationProperties(string provider, string redirectUrl);
		Task<LoginResult> PerfromExternalLogin();
		Task<IEnumerable<UserLoginInfo>> GetExternalLogins(ClaimsPrincipal userProperty);
		Task AddExternalLogin(ClaimsPrincipal userProperty);
		Task RemoveExternalLogin(ClaimsPrincipal userProperty, string provider);
		Task<User> GetCurrentUser(ClaimsPrincipal userProperty);
		Task<IEnumerable<string>> GetCurrentRoles(ClaimsPrincipal userProperty);
		Task Logout();
	}
	internal class AccountRepository : IAccountRepository
	{
		private MintPlayerContext mintplayer_context;
		private UserManager<Entities.User> user_manager;
		private SignInManager<Entities.User> signin_manager;
		private JwtIssuerOptions jwtIssuerOptions;
		public AccountRepository(UserManager<Entities.User> user_manager, SignInManager<Entities.User> signin_manager, MintPlayerContext mintplayer_context, IOptions<JwtIssuerOptions> jwtIssuerOptions)
		{
			this.user_manager = user_manager;
			this.signin_manager = signin_manager;
			this.mintplayer_context = mintplayer_context;
			this.jwtIssuerOptions = jwtIssuerOptions.Value;
		}

		public async Task<Tuple<User, string>> Register(User user, string password)
		{
			try
			{
				var entity_user = ToEntity(user);
				var identity_result = await user_manager.CreateAsync(entity_user, password);
				if (identity_result.Succeeded)
				{
					var dto_user = ToDto(entity_user, true);
					var confirmation_token = await user_manager.GenerateEmailConfirmationTokenAsync(entity_user);
					return new Tuple<User, string>(dto_user, confirmation_token);
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

		public async Task<LoginResult> LocalLogin(string email, string password, bool createCookie)
		{
			try
			{
				var user = await user_manager.FindByEmailAsync(email);
				if (user == null)
					throw new LoginException();

				var checkPasswordResult = await signin_manager.CheckPasswordSignInAsync(user, password, true);
                if (!checkPasswordResult.Succeeded)
                {
                    throw new LoginException();
                }

                if (createCookie)
                {
                    var signinResult = await signin_manager.PasswordSignInAsync(user, password, true, true);
                    if (!signinResult.Succeeded)
                    {
                        throw new LoginException();
                    }

                    return new LoginResult
                    {
                        Status = true,
                        Platform = "local",
                        User = ToDto(user, true)
                    };
                }
                else
                {
                    return new LoginResult
                    {
                        Status = true,
                        Platform = "local",
                        User = ToDto(user, true),
                        Token = CreateToken(user)
                    };
                }

            }
			catch (LoginException loginEx)
			{
				throw loginEx;
			}
			catch (Exception ex)
			{
				throw new LoginException(ex);
			}
		}

		public async Task<IEnumerable<AuthenticationScheme>> GetExternalLoginProviders()
		{
			// https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade
			// https://console.developers.google.com
			// https://developers.facebook.com/apps
			// http://developer.twitter.com/en/apps

			var providers = await signin_manager.GetExternalAuthenticationSchemesAsync();
			return providers.ToList();
		}

		public Task<AuthenticationProperties> ConfigureExternalAuthenticationProperties(string provider, string redirectUrl)
		{
			var properties = signin_manager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return Task.FromResult(properties);
		}

		public async Task<LoginResult> PerfromExternalLogin()
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
					PictureUrl = null
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

			await signin_manager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);
			return new LoginResult
			{
				Status = true,
				Platform = info.LoginProvider,
				User = ToDto(user, true)
			};
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
			return ToDto(user, true);
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
				SigningCredentials = new Func<SigningCredentials>(() => {
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
		#region Conversion methods
		internal static Entities.User ToEntity(User user)
		{
			if (user == null) return null;
			return new Entities.User
			{
				Id = user.Id,
				Email = user.Email,
				UserName = user.UserName,
				PictureUrl = user.PictureUrl
			};
		}
		internal static User ToDto(Entities.User user, bool mapSensitiveData)
		{
			if (user == null) return null;
			return new User
			{
				Id = mapSensitiveData ? user.Id : Guid.Empty,
				Email = mapSensitiveData ? user.Email : null,
				UserName = user.UserName,
				PictureUrl = user.PictureUrl
			};
		}
		#endregion
	}
}
