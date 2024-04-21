using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Data.Exceptions.Account;
using MintPlayer.Web.Server.ViewModels.Account;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MintPlayer.Data.Abstractions.Services;

namespace MintPlayer.Web.Server.Controllers.Api;

[ApiController]
[Route("api/v1/[controller]")]
public class AccountController : Controller
{
	private IAccountService accountService;
	public AccountController(IAccountService accountService)
	{
		this.accountService = accountService;
	}

	/// <summary>Registers a new MintPlayer user.</summary>
	/// <param name="registrationInformation">Registration data for the new user.</param>
	[HttpPost("register", Name = "api-account-register")]
	public async Task<ActionResult> Register([FromBody] UserDataVM registrationInformation)
	{
		try
		{
			await accountService.Register(registrationInformation.User, registrationInformation.Password);
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

	/// <summary>Attempts to login a user and generates a Bearer token for that user.</summary>
	/// <param name="loginInfo">Login credentials for the user.</param>
	/// <returns>An object containing the status, user and token. If the login failed contains the error.</returns>
	[HttpPost("login", Name = "api-account-login")]
	//[Produces("application/json", "text/json", "application/xml", "text/xml")]
	public async Task<ActionResult<LoginResult>> Login([FromBody] LoginVM loginInfo)
	{
		try
		{
			var login_result = await accountService.LocalLogin(loginInfo.Email, loginInfo.Password, false);
			return Ok(login_result);
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

	/// <summary>Check who is currently signed in.</summary>
	/// <returns>The currently signed in user.</returns>
	[HttpGet("current-user", Name = "api-account-currentuser")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<User>> GetCurrentUser()
	{
		var user = await accountService.GetCurrentUser(User);
		return Ok(user);
	}

	/// <summary>Check the roles for the current user.</summary>
	/// <returns>A list of roles for the currently signed-in user.</returns>
	[HttpGet("roles", Name = "api-account-roles")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
	{
		var roles = await accountService.GetCurrentRoles(User);
		return Ok(roles);
	}
}
