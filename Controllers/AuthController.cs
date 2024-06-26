using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using pokerapi.Models;
using pokerapi.Interfaces;
using pokerapi.Services;

namespace pokerapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationModel userModel)
        {
            try
            {
                var token = await _authService.Register(userModel);
                return Ok(new { token = token, message = "Registration successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginModel userModel)
        {
            try
            {
                var token = await _authService.Login(userModel);
                var playerExists = await _authService.PlayerExists(userModel.Username);
                var redirectUrl = playerExists ? "/lobby" : "/join";
                return Ok(new { token, redirectUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
