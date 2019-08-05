using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Data.Dtos;
using MintPlayer.Data.Exceptions.Account;
using MintPlayer.Data.Repositories.Interfaces;
using MintPlayer.Web.ViewModels.Account;

namespace MintPlayer.Web.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private IAccountRepository accountRepository;
        public AccountController(IAccountRepository accountRepository)
        {
            this.accountRepository = accountRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserDataVM userCreateVM)
        {
            try
            {
                var registration_data = await accountRepository.Register(userCreateVM.User, userCreateVM.Password);
                var user = registration_data.Item1;
                var confirmation_token = registration_data.Item2;
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]LoginVM loginVM)
        {
            try
            {
                var login_result = await accountRepository.LocalLogin(loginVM.Email, loginVM.Password, true);
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

        [Authorize]
        [HttpGet("current-user")]
        public async Task<User> GetCurrentUser()
        {
            var user = await accountRepository.GetCurrentUser(User);
            return user;
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logoout()
        {
            await accountRepository.Logout();
            return Ok();
        }
    }
}