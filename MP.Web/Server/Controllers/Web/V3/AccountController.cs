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
using MintPlayer.AspNetCore.SpaServices.Routing;
using System.Text.Encodings.Web;
using MintPlayer.Data.Exceptions.Account.TwoFactor;
using Microsoft.AspNetCore.Hosting;

namespace MintPlayer.Web.Server.Controllers.Web.V3
{
	[Controller]
	[Route("web/v3/[controller]")]
	public class AccountController : Controller
	{
		private readonly IAccountService accountService;
		private readonly ISpaRouteService spaRouteService;
		private readonly UrlEncoder urlEncoder;
		private readonly IWebHostEnvironment webHostEnvironment;
		public AccountController(IAccountService accountService, ISpaRouteService spaRouteService, UrlEncoder urlEncoder, IWebHostEnvironment webHostEnvironment)
		{
			this.accountService = accountService;
			this.spaRouteService = spaRouteService;
			this.urlEncoder = urlEncoder;
			this.webHostEnvironment = webHostEnvironment;
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
		[ValidateAntiForgeryToken]
		[HttpPost("register", Name = "web-v3-account-register")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult> Register([FromBody] UserDataVM userCreateVM)
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

		[ValidateAntiForgeryToken]
		[HttpPost("verify/resend", Name = "web-v3-account-verify-resend")]
		[ApiExplorerSettings(IgnoreApi = true)]
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
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult> Verify([FromQuery] string email, [FromQuery] string code)
		{
			try
			{
				var decodedCode = System.Text.Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(code));
				await accountService.VerifyEmailConfirmationToken(email, decodedCode);
				var loginUrl = await spaRouteService.GenerateUrl("account-login", new { });
				return Redirect(loginUrl);
			}
			catch (VerifyEmailException verifyEx)
			{
				return StatusCode(501);
			}
			catch (Exception ex)
			{
				return StatusCode(500);
			}
		}

		[ValidateAntiForgeryToken]
		[HttpPost("password/reset", Name = "web-v3-account-resetpassword-request")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult> RequestResetPassword([FromBody] RequestResetPasswordVM model)
		{
			try
			{
				var token = await accountService.GeneratePasswordResetToken(model.Email);
				var encodedToken = Base64UrlTextEncoder.Encode(System.Text.Encoding.UTF8.GetBytes(token));
				var resetUrl = await spaRouteService.GenerateUrl("account-passwordreset-perform", new { email = model.Email, token = encodedToken }, HttpContext);
				await accountService.SendPasswordResetEmail(model.Email, resetUrl);
				return Ok();
			}
			catch (Exception)
			{
				return StatusCode(500);
			}
		}

		//[HttpGet("password/reset", Name = "web-v3-account-resetpassword-form")]
		//[ApiExplorerSettings(IgnoreApi = true)]
		//public async Task<ActionResult> ResetPassword([FromQuery] string email, [FromQuery] string token)
		//{
		//	try
		//	{
		//		var url = spaRouteService.GenerateUrl("")
		//	}
		//	catch (Exception)
		//	{
		//		return StatusCode(500);
		//	}
		//}

		[HttpPut("password/reset", Name = "web-v3-account-resetpassword-reset")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordVM model)
		{
			try
			{
				if (model.NewPassword != model.NewPasswordConfirmation)
				{
					return Forbid();
				}

				var token = System.Text.Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(model.Token));
				await accountService.ResetPassword(model.Email, token, model.NewPassword);
				return Ok();
			}
			catch (Exception ex)
			{
				return StatusCode(500);
			}
		}

		// POST: web/Account/login
		[ValidateAntiForgeryToken]
		[HttpPost("login", Name = "web-v3-account-login")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<LoginResult>> Login([FromBody] LoginVM loginVM)
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
			catch (EmailNotConfirmedException confirmEx)
			{
				return StatusCode(403);
			}
			catch (Exception ex)
			{
				return StatusCode(500);
			}
		}

		[Authorize]
		[HttpPost("two-factor-registration", Name = "web-v3-account-twofactor-registration")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<TwoFactorRegistrationUrlVM>> GetTwoFactorRegistrationUrl()
		{
			const string appName = "MintPlayer";
			var user = await accountService.GetCurrentUser(User);
			var image = "https://mintplayer.com/assets/logo/music_note_120.png";
			var registrationCode = await accountService.GenerateTwoFactorRegistrationCode(User);
			var registrationUrl = $"otpauth://totp/{urlEncoder.Encode(appName)}:{urlEncoder.Encode(user.Email)}?secret={registrationCode}&issuer={urlEncoder.Encode(appName)}&image={urlEncoder.Encode(image)}&digits=6";

			return Ok(new TwoFactorRegistrationUrlVM
			{
				RegistrationUrl = registrationUrl,
			});
		}

		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPost("two-factor-setup", Name = "web-v3-account-twofactor-setup")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<IEnumerable<string>>> SetEnableTwoFactor([FromBody] TwoFactorSetupVM twoFactorSetup)
		{
			try
			{
				await accountService.SetTwoFactorEnabled(User, twoFactorSetup.SetupCode, twoFactorSetup.Enabled);
				if (twoFactorSetup.Enabled)
				{
					var backupCodes = await accountService.GenerateTwoFactorBackupCodes(User);
					return Ok(backupCodes);
				}
				else
				{
					return Ok();
				}
			}
			catch (InvalidTwoFactorCodeException invEx)
			{
				return StatusCode(401);
			}
			catch (TwoFactorSetupException twoFactorEx)
			{
				return StatusCode(500);
			}
			catch (Exception ex)
			{
				return StatusCode(500);
			}
		}

		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPut("two-factor-bypass", Name = "web-v3-account-twofactor-bypass")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult> BypassTwoFactorForExternalLogins([FromBody] TwoFactorBypassVM twoFactorBypass)
		{
			try
			{
				await accountService.SetTwoFactorBypass(User, twoFactorBypass.Bypass2faForExternalLogins, twoFactorBypass.SetupCode);
				return Ok();
			}
			catch (InvalidTwoFactorCodeException invEx)
			{
				return StatusCode(401);
			}
			catch (TwoFactorSetupException twoFactorEx)
			{
				return StatusCode(500);
			}
			catch (Exception ex)
			{
				return StatusCode(500);
			}
		}

		[ValidateAntiForgeryToken]
		[HttpPost("two-factor-login")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<User> TwoFactorLogin([FromBody] TwoFactorLoginVM twoFactorLoginVM)
		{
			var user = await accountService.TwoFactorLogin(twoFactorLoginVM.Code, twoFactorLoginVM.Remember);
			return user;
		}

		[ValidateAntiForgeryToken]
		[HttpPost("two-factor-recovery")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult> TwoFactorRecovery([FromBody] TwoFactorRecoveryVM twoFactorRecoveryVM)
		{
			try
			{
				await accountService.TwoFactorRecovery(twoFactorRecoveryVM.Code);
				return Ok();
			}
			catch (TwoFactorRecoveryException)
			{
				return Unauthorized();
			}
			catch (Exception)
			{
				return StatusCode(500);
			}
		}

		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPost("two-factor-generate-new-codes")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<IEnumerable<string>>> TwoFactorGenerateCodes()
		{
			try
			{
				var backupCodes = await accountService.GenerateTwoFactorBackupCodes(User);
				return Ok(backupCodes);
			}
			catch (Exception)
			{
				return StatusCode(500);
			}
		}


		[Authorize]
		[HttpGet("two-factor-recovery-remaining-codes")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<int>> TwoFactorGetRemainingNumberOfCodes()
		{
			try
			{
				var numberOfBackupCodes = await accountService.GetRemainingNumberOfRecoveryCodes(User);
				return Ok(numberOfBackupCodes);
			}
			catch (Exception)
			{
				return StatusCode(500);
			}
		}

		// GET: web/Account/providers
		[AllowAnonymous]
		[HttpGet("providers", Name = "web-v3-account-external-providers")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<IEnumerable<string>>> Providers()
		{
			var result = await accountService.GetProviders();
			return Ok(result);
		}

		// GET: web/Account/connect/{provider}
		[AllowAnonymous]
		[HttpGet("connect/{medium}/{provider}", Name = "web-v3-account-external-connect-challenge")]
		[ApiExplorerSettings(IgnoreApi = true)]
#if RELEASE
        [Host("external.mintplayer.com")]
#endif
		public async Task<ActionResult> ExternalLogin([FromRoute] string medium, [FromRoute] string provider)
		{
			var redirectUrl = Url.RouteUrl("web-v3-account-external-connect-callback", new { medium, provider });
			var properties = await accountService.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return Challenge(properties, provider);
		}

		// GET: web/Account/connect/{provider}/callback
		[HttpGet("connect/{medium}/{provider}/callback", Name = "web-v3-account-external-connect-callback")]
		[ApiExplorerSettings(IgnoreApi = true)]
#if RELEASE
        [Host("external.mintplayer.com")]
#endif
		public async Task<ActionResult> ExternalLoginCallback([FromRoute] string medium, [FromRoute] string provider)
		{
			try
			{
				var login_result = await accountService.PerfromExternalLogin();
				switch (login_result.Status)
				{
					case Dtos.Enums.LoginStatus.Success:
						var successModel = new ExternalLoginResultVM
						{
							Status = login_result.Status,
							Medium = medium,
							Platform = login_result.Platform
						};
						return View(successModel);
					case Dtos.Enums.LoginStatus.RequiresTwoFactor:
						// For external logins, show the two-factor input form in the popup.
						return RedirectToAction(nameof(ExternalLoginTwoFactor), new { medium, provider = login_result.Platform });
					default:
						var failedModel = new ExternalLoginResultVM
						{
							Status = login_result.Status,
							Medium = medium,
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
					Status = Dtos.Enums.LoginStatus.Failed,
					Medium = medium,
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
					Status = Dtos.Enums.LoginStatus.Failed,
					Medium = medium,
					Platform = provider,

					Error = "Could not login",
					ErrorDescription = "There was an error with your social login"
				};
				return View(model);
			}
		}

#if RELEASE
        [Host("external.mintplayer.com")]
#endif
		[HttpGet("two-factor-login-external/{medium}/{provider}", Name = "web-v3-account-external-twofactor")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public ActionResult ExternalLoginTwoFactor([FromRoute] string medium, [FromRoute] string provider)
		{
			var root = webHostEnvironment.ContentRootPath + "/ClientApp/dist";
			var files = System.IO.Directory.GetFiles(root, "styles.*.css");
			var angularStylesheet = files.Any() ? Url.Content($"~/{System.IO.Path.GetFileName(files.First())}") : null;

			var model = new ExternalLoginTwoFactorVM
			{
				SubmitUrl = Url.Action(nameof(ExternalLoginTwoFactorCallback), new { medium, provider }),
				StylesheetUrl = angularStylesheet
			};
			return View(model);
		}

#if RELEASE
        [Host("external.mintplayer.com")]
#endif
		[ValidateAntiForgeryToken]
		[HttpPost("two-factor-login-external/{medium}/{provider}", Name = "web-v3-account-external-twofactor-callback")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult> ExternalLoginTwoFactorCallback([FromRoute] string medium, [FromRoute] string provider, [FromForm] ExternalLoginTwoFactorVM externalLoginTwoFactorVM)
		{
			try
			{
				var user = await accountService.TwoFactorLogin(externalLoginTwoFactorVM.Code, externalLoginTwoFactorVM.Remember);
				if (user == null) throw new Exception();

				var successModel = new ExternalLoginResultVM
				{
					Status = Dtos.Enums.LoginStatus.Success,
					Medium = medium,
					Platform = provider,
					User = user
				};
				return View(nameof(ExternalLoginCallback), successModel);
			}
			catch (Exception)
			{
				return RedirectToAction(nameof(ExternalLoginTwoFactor), new { medium, provider });
			}

		}

		// GET: web/Account/logins
		[Authorize]
		[HttpGet("logins", Name = "web-v3-account-external-logins")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<IEnumerable<string>>> GetExternalLogins()
		{
			var logins = await accountService.GetExternalLogins(User);
			return Ok(logins.Select(l => l.ProviderDisplayName));
		}

		// GET: web/Account/add/{provider}
		[Authorize]
		[HttpGet("add/{medium}/{provider}", Name = "web-v3-account-external-add-challenge")]
		[ApiExplorerSettings(IgnoreApi = true)]
#if RELEASE
		[Host("external.mintplayer.com")]
#endif
		public async Task<ActionResult> AddExternalLogin([FromRoute] string medium, [FromRoute] string provider)
		{
			var redirectUrl = Url.RouteUrl("web-v3-account-external-add-callback", new { medium, provider });
			var properties = await accountService.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return Challenge(properties, provider);
		}

		// GET: web/Account/add/{provider}/callback
		[Authorize]
		//[ValidateAntiForgeryToken]
		[HttpGet("add/{medium}/{provider}/callback", Name = "web-v3-account-external-add-callback")]
		[ApiExplorerSettings(IgnoreApi = true)]
#if RELEASE
		[Host("external.mintplayer.com")]
#endif
		public async Task<ActionResult> AddExternalLoginCallback([FromRoute] string medium, [FromRoute] string provider)
		{
			try
			{
				await accountService.AddExternalLogin(User);
				var model = new ExternalLoginResultVM
				{
					Status = Dtos.Enums.LoginStatus.Success,
					Medium = medium,
					Platform = provider
				};
				return View(model);
			}
			catch (Exception)
			{
				var model = new ExternalLoginResultVM
				{
					Status = Dtos.Enums.LoginStatus.Failed,
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
		[ValidateAntiForgeryToken]
		[HttpDelete("logins/{provider}", Name = "web-v3-account-external-delete")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult> DeleteLogin(string provider)
		{
			await accountService.RemoveExternalLogin(User, provider);
			return Ok();
		}

		// GET: web/Account/current-user
		[Authorize]
		[HttpGet("current-user", Name = "web-v3-account-currentuser")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<User>> GetCurrentUser()
		{
			var user = await accountService.GetCurrentUser(User);
			return Ok(user);
		}

		[Authorize]
		[HttpGet("password", Name = "web-v3-account-has-password")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<bool>> GetHasPassword()
		{
			var hasPassword = await accountService.GetHasPassword(User);
			return Ok(hasPassword);
		}

		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPut("password", Name = "web-v3-account-update-password")]
		[ApiExplorerSettings(IgnoreApi = true)]
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
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
		{
			var roles = await accountService.GetCurrentRoles(User);
			return Ok(roles);
		}

		// POST: web/Account/logout
		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPost("logout", Name = "web-v3-account-logout")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult> Logout()
		{
			await accountService.Logout();
			return Ok();
		}

		[HttpPost("csrf-refresh", Name = "web-v3-account-csrf-refresh")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<ActionResult> RefreshCsrfToken()
		{
			// Just an empty method that returns a new cookie with a new CSRF token.
			// Call this method when the user has signed in/out.
			await Task.Delay(5);

			return Ok();
		}
	}
}
