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

namespace MintPlayer.Web.Server.Controllers.Web.V1
{
	[Controller]
	[Route("web/v1/[controller]")]
	public class AccountController : Controller
	{
		private IAccountService accountService;
		public AccountController(IAccountService accountService)
		{
			this.accountService = accountService;
		}

		/**
		 * 
		 * Microsoft: https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade
		 * Google: https://console.developers.google.com/
		 * Facebook: https://developers.facebook.com/apps
		 * Twitter: https://developer.twitter.com/en/apps
		 * 
		 **/

		// POST: web/Account/register
		[HttpPost("register", Name = "web-v1-account-register")]
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

		// POST: web/Account/login
		[HttpPost("login", Name = "web-v1-account-login")]
		public async Task<ActionResult<LocalLoginResult>> Login([FromBody]LoginVM loginVM)
		{
			try
			{
				var login_result = await accountService.LocalLogin(loginVM.Email, loginVM.Password, true);
                switch (login_result.Status)
                {
                    case Dtos.Enums.LoginStatus.Success:
                    case Dtos.Enums.LoginStatus.RequiresTwoFactor:
						return Ok(login_result);
					default:
						throw new Exception();
                }
			}
			catch (LoginException loginEx)
			{
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return StatusCode(500);
			}
		}

		// GET: web/Account/providers
		[AllowAnonymous]
		[HttpGet("providers", Name = "web-v1-account-external-providers")]
		public async Task<ActionResult<IEnumerable<string>>> Providers()
		{
			var result = await accountService.GetProviders();
			return Ok(result);
		}

		// GET: web/Account/connect/{provider}
		[AllowAnonymous]
		[HttpGet("connect/{provider}", Name = "web-v1-account-external-connect-challenge")]
		public async Task<ActionResult> ExternalLogin(string provider)
		{
			var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { provider });
			var properties = await accountService.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return Challenge(properties, provider);
		}

		// GET: web/Account/connect/{provider}/callback
		[HttpGet("connect/{provider}/callback", Name = "web-v1-account-external-connect-callback")]
		public async Task<ActionResult> ExternalLoginCallback([FromRoute]string provider)
		{
			try
			{
				var login_result = await accountService.PerfromExternalLogin();
                switch (login_result.Status)
                {
                    case Dtos.Enums.LoginStatus.Success:
						var successModel = new ExternalLoginResultVM
						{
							Status = true,
							Platform = login_result.Platform
						};
						return View(successModel);
					default:
						var failedModel = new ExternalLoginResultVM
						{
							Status = false,
							Platform = login_result.Platform,

							Error = login_result.Error,
							ErrorDescription = login_result.ErrorDescription
						};
						return View(failedModel);
				}
			}
			catch (OtherAccountException otherAccountEx)
			{
				var model = new ExternalLoginResultVM
				{
					Status = false,
					Platform = provider,

					Error = "Could not login",
					ErrorDescription = otherAccountEx.Message
				};
				return View(model);
			}
			catch (Exception ex)
			{
				var model = new ExternalLoginResultVM
				{
					Status = false,
					Platform = provider,

					Error = "Could not login",
					ErrorDescription = "There was an error with your social login"
				};
				return View(model);
			}
		}

		// GET: web/Account/logins
		[Authorize]
		[HttpGet("logins", Name = "web-v1-account-external-logins")]
		public async Task<ActionResult<IEnumerable<string>>> GetExternalLogins()
		{
			var logins = await accountService.GetExternalLogins(User);
			return Ok(logins.Select(l => l.ProviderDisplayName));
		}

		// GET: web/Account/add/{provider}
		[Authorize]
		[HttpGet("add/{provider}", Name = "web-v1-account-external-add-challenge")]
		public async Task<ActionResult> AddExternalLogin(string provider)
		{
			var redirectUrl = Url.Action(nameof(AddExternalLoginCallback), "Account", new { provider });
			var properties = await accountService.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return Challenge(properties, provider);
		}

		// GET: web/Account/add/{provider}/callback
		[Authorize]
		[HttpGet("add/{provider}/callback", Name = "web-v1-account-external-add-callback")]
		public async Task<ActionResult> AddExternalLoginCallback([FromRoute]string provider)
		{
			try
			{
				await accountService.AddExternalLogin(User);
				var model = new ExternalLoginResultVM
				{
					Status = true,
					Platform = provider
				};
				return View(model);
			}
			catch (Exception)
			{
				var model = new ExternalLoginResultVM
				{
					Status = false,
					Platform = provider,

					Error = "Could not login",
					ErrorDescription = "There was an error with your social login"
				};
				return View(model);
			}
		}

		// DELETE: web/Account/logins/{provider}
		[Authorize]
		[HttpDelete("logins/{provider}", Name = "web-v1-account-external-delete")]
		public async Task<ActionResult> DeleteLogin(string provider)
		{
			await accountService.RemoveExternalLogin(User, provider);
			return Ok();
		}

		// GET: web/Account/current-user
		[Authorize]
		[HttpGet("current-user", Name = "web-v1-account-currentuser")]
		public async Task<ActionResult<User>> GetCurrentUser()
		{
			var user = await accountService.GetCurrentUser(User);
			return Ok(user);
		}

		// POST: web/Account/logout
		[Authorize]
		[HttpPost("logout", Name = "web-v1-account-logout")]
		public async Task<ActionResult> Logout()
		{
			await accountService.Logout();
			return Ok();
		}
	}
}