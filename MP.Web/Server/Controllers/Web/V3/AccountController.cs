using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Exceptions.Account;
using MintPlayer.Data.Exceptions.Account.ExternalLogin;
using MintPlayer.Data.Repositories;
using MintPlayer.Data.Services;
using MintPlayer.Web.Server.ViewModels.Account;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authentication;

namespace MintPlayer.Web.Server.Controllers.Web.V3
{
	[Controller]
	[Route("web/v3/[controller]")]
	public class AccountController : Controller
	{
		private IAccountService accountService;
		public AccountController(IAccountService accountService)
		{
			this.accountService = accountService;
		}

		/**
		 * 
		 * Microsoft: https://medium.com/r/?url=https%3A%2F%2Fportal.azure.com%2F%23blade%2FMicrosoft_AAD_RegisteredApps%2FApplicationsListBlade
		 * Google: https://console.developers.google.com/
		 * Facebook: https://developers.facebook.com/apps
		 * Twitter: https://developer.twitter.com/en/apps
		 * 
		 **/

		// POST: web/Account/register
		[HttpPost("register", Name = "web-v3-account-register")]
		public async Task<ActionResult> Register([FromBody]UserDataVM userCreateVM)
		{
			try
			{
				await accountService.Register(userCreateVM.User, userCreateVM.Password);
				return Ok();
			}
			catch (RegistrationException registrationEx)
			{
				return BadRequest(registrationEx.Errors.Select(e => e.Description));
			}
			catch (Exception ex)
			{
				return StatusCode(500);
			}
		}

		[HttpPost("verify/resend", Name = "web-v3-account-verify-resend")]
		public async Task<ActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailVM model)
		{
            try
            {
				await accountService.SendConfirmationEmail(model.Email);
				return Ok();
            }
            catch (Exception)
            {
				return StatusCode(500);
            }
		}

		[HttpGet("verify", Name = "web-v3-account-verify")]
		public async Task<ActionResult> Verify([FromQuery] string email, [FromQuery] string code)
        {
            try
            {
				var decodedCode = System.Text.Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(code));
				await accountService.VerifyEmailConfirmationToken(email, decodedCode);
				return Ok();
            }
			catch(VerifyEmailException verifyEx)
            {
				return StatusCode(501);
			}
			catch (Exception ex)
            {
				return StatusCode(500);
            }
        }

		// POST: web/Account/login
		[HttpPost("login", Name = "web-v3-account-login")]
		public async Task<ActionResult<LoginResult>> Login([FromBody]LoginVM loginVM)
		{
			try
			{
				var login_result = await accountService.LocalLogin(loginVM.Email, loginVM.Password, true);
				return Ok(login_result);
			}
			catch (LoginException loginEx)
			{
				return Unauthorized();
			}
			catch (EmailNotConfirmedException confirmEx)
            {
				return StatusCode(403);
            }
			catch (Exception ex)
			{
				return StatusCode(500);
			}
		}

		// GET: web/Account/providers
		[AllowAnonymous]
		[HttpGet("providers", Name = "web-v3-account-external-providers")]
		public async Task<ActionResult<IEnumerable<string>>> Providers()
		{
			var result = await accountService.GetProviders();
			return Ok(result);
		}

		// GET: web/Account/connect/{provider}
		[AllowAnonymous]
		[HttpGet("connect/{medium}/{provider}", Name = "web-v3-account-external-connect-challenge")]
#if RELEASE
        [Host("external.mintplayer.com")]
#endif
        public async Task<ActionResult> ExternalLogin([FromRoute]string medium, [FromRoute]string provider)
		{
			//var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { medium, provider });
			var redirectUrl = Url.RouteUrl("web-v3-account-external-connect-callback", new { medium, provider });
			var properties = await accountService.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return Challenge(properties, provider);
		}

		// GET: web/Account/connect/{provider}/callback
		[HttpGet("connect/{medium}/{provider}/callback", Name = "web-v3-account-external-connect-callback")]
#if RELEASE
        [Host("external.mintplayer.com")]
#endif
        public async Task<ActionResult> ExternalLoginCallback([FromRoute]string medium, [FromRoute]string provider)
		{
			try
			{
				var login_result = await accountService.PerfromExternalLogin();
				if (login_result.Status)
				{
					var model = new LoginResultVM
					{
						Status = true,
						Medium = medium,
						Platform = login_result.Platform
					};
					return View(model);
				}
				else
				{
					var model = new LoginResultVM
					{
						Status = false,
						Medium = medium,
						Platform = login_result.Platform,

						Error = login_result.Error,
						ErrorDescription = login_result.ErrorDescription
					};
					return View(model);
				}
			}
			catch (OtherAccountException otherAccountEx)
			{
				var model = new LoginResultVM
				{
					Status = false,
					Medium = medium,
					Platform = provider,

					Error = "Could not login",
					ErrorDescription = otherAccountEx.Message
				};
				return View(model);
			}
			catch (Exception ex)
			{
				var model = new LoginResultVM
				{
					Status = false,
					Medium = medium,
					Platform = provider,

					Error = "Could not login",
					ErrorDescription = "There was an error with your social login"
				};
				return View(model);
			}
		}

		// GET: web/Account/logins
		[Authorize]
		[HttpGet("logins", Name = "web-v3-account-external-logins")]
		public async Task<ActionResult<IEnumerable<string>>> GetExternalLogins()
		{
			var logins = await accountService.GetExternalLogins(User);
			return Ok(logins.Select(l => l.ProviderDisplayName));
		}

		// GET: web/Account/add/{provider}
		[Authorize]
		[HttpGet("add/{medium}/{provider}", Name = "web-v3-account-external-add-challenge")]
#if RELEASE
		[Host("external.mintplayer.com")]
#endif
		public async Task<ActionResult> AddExternalLogin([FromRoute]string medium, [FromRoute]string provider)
		{
			var redirectUrl = Url.RouteUrl("web-v3-account-external-add-callback", new { medium, provider });
			var properties = await accountService.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return Challenge(properties, provider);
		}

		// GET: web/Account/add/{provider}/callback
		[Authorize]
		[HttpGet("add/{medium}/{provider}/callback", Name = "web-v3-account-external-add-callback")]
#if RELEASE
		[Host("external.mintplayer.com")]
#endif
		public async Task<ActionResult> AddExternalLoginCallback([FromRoute]string medium, [FromRoute]string provider)
		{
			try
			{
				await accountService.AddExternalLogin(User);
				var model = new LoginResultVM
				{
					Status = true,
					Medium = medium,
					Platform = provider
				};
				return View(model);
			}
			catch (Exception)
			{
				var model = new LoginResultVM
				{
					Status = false,
					Medium = medium,
					Platform = provider,

					Error = "Could not login",
					ErrorDescription = "There was an error with your social login"
				};
				return View(model);
			}
		}

		// DELETE: web/Account/logins/{provider}
		[Authorize]
		[HttpDelete("logins/{provider}", Name = "web-v3-account-external-delete")]
		public async Task<ActionResult> DeleteLogin(string provider)
		{
			await accountService.RemoveExternalLogin(User, provider);
			return Ok();
		}

		// GET: web/Account/current-user
		[Authorize]
		[HttpGet("current-user", Name = "web-v3-account-currentuser")]
		public async Task<ActionResult<User>> GetCurrentUser()
		{
			var user = await accountService.GetCurrentUser(User);
			return Ok(user);
		}

		[Authorize]
		[HttpGet("password", Name = "web-v3-account-has-password")]
		public async Task<ActionResult<bool>> GetHasPassword()
		{
			var hasPassword = await accountService.GetHasPassword(User);
			return Ok(hasPassword);
		}

		[Authorize]
		[HttpPut("password", Name = "web-v3-account-update-password")]
		public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordVM updatePassword)
        {
            try
			{
				await accountService.UpdatePassword(User, updatePassword.CurrentPassword, updatePassword.NewPassword, updatePassword.Confirmation);
				return Ok();
			}
			catch (ChangePasswordException passwordEx)
            {
				return StatusCode(500);
            }
            catch (Exception ex)
            {
				return StatusCode(500);
			}
		}

		// GET: web/Account/roles
		[Authorize]
		[HttpGet("roles", Name = "web-v3-account-roles")]
		public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
		{
			var roles = await accountService.GetCurrentRoles(User);
			return Ok(roles);
		}

		// POST: web/Account/logout
		[Authorize]
		[HttpPost("logout", Name = "web-v3-account-logout")]
		public async Task<ActionResult> Logout()
		{
			await accountService.Logout();
			return Ok();
		}
	}
}